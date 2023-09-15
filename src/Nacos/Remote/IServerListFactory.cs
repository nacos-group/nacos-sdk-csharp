namespace Nacos.Remote
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Exceptions;
    using Nacos.Logging;
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IServerListFactory : IDisposable
    {
        string GetName();

        string GenNextServer();

        string GetCurrentServer();

        List<string> GetServerList();
    }

    public class ServerListManager : IServerListFactory
    {
        private static HttpClient _httpClient = new HttpClient();

        public const string FIXED_NAME = "fixed";
        private const string DefaultNodesPath = "serverlist";
        private const int DEFAULT_TIMEOUT = 5000;
        private const int DefaultEndpointPort = 8080;

        private readonly ILogger _logger = NacosLogManager.CreateLogger<ServerListManager>();

        private long _refreshServerListInternal = 30000;

        private int _currentIndex = 0;

        private List<string> _serversFromEndpoint = new List<string>();

        private List<string> _serverList = new List<string>();

        private Timer _refreshServerListTimer;

        private string _endpoint;

        private string _nacosDomain;

        private string _addressServerUrl;

        private long _lastServerListRefreshTime = 0L;

        private string _namespace;
        private bool _isFixed = false;
        private string _name = "";
        private string _contentPath;

        public ServerListManager(NacosSdkOptions options)
        {
            InitServerAddr(options);
        }

        public ServerListManager(IOptions<NacosSdkOptions> optionsAccs)
        {
            var options = optionsAccs.Value;
            InitServerAddr(options);
        }

        private void InitServerAddr(NacosSdkOptions options)
        {
            _namespace = string.IsNullOrWhiteSpace(options.Namespace) ? Common.Constants.DEFAULT_NAMESPACE_ID : options.Namespace;
            _contentPath = options.ContextPath;
            _endpoint = options.EndPoint;

            if (options.ServerAddresses != null && options.ServerAddresses.Any())
            {
                _isFixed = true;
                foreach (var item in options.ServerAddresses)
                {
                    // here only trust the input server addresses of user
                    _serverList.Add(item.TrimEnd('/'));
                }

                if (_namespace.IsNullOrWhiteSpace())
                {
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverList)}";
                }
                else
                {
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverList)}-{_namespace}";
                }

                if (_serverList.Count == 1)
                {
                    _nacosDomain = _serverList[0];
                }
            }
            else
            {
                if (_endpoint.IsNullOrWhiteSpace())
                {
                    throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "endpoint is blank");
                }

                _isFixed = false;

                var endpoint = _endpoint.IndexOf(':') == -1 ? $"{_endpoint}:{DefaultEndpointPort}" : _endpoint;

                if (_namespace.IsNullOrWhiteSpace())
                {
                    _name = _endpoint;
                    _addressServerUrl = $"http://{endpoint}/{_contentPath}/{DefaultNodesPath}";
                }
                else
                {
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverList)}-{_namespace}";

                    _addressServerUrl = $"http://{endpoint}/{_contentPath}/{DefaultNodesPath}?namespace={_namespace}";
                }

                _serversFromEndpoint = GetServerListFromEndpoint()
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                _refreshServerListTimer = new Timer(
                    async x =>
                    {
                        await RefreshSrvIfNeedAsync().ConfigureAwait(false);
                    }, null, 0, _refreshServerListInternal);
            }
        }

        private async Task<List<string>> GetServerListFromEndpoint()
        {
            var list = new List<string>();
            try
            {
                var header = Nacos.Naming.Utils.NamingHttpUtil.BuildHeader();

                using var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(DEFAULT_TIMEOUT));

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, _addressServerUrl);
                foreach (var item in header) req.Headers.TryAddWithoutValidation(item.Key, item.Value);

                var resp = await _httpClient.SendAsync(req, cts.Token).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception($"Error while requesting: {_addressServerUrl} . Server returned: {resp.StatusCode}");
                }

                var str = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                using StringReader sr = new StringReader(str);
                while (true)
                {
                    var line = await sr.ReadLineAsync().ConfigureAwait(false);
                    if (line == null || line.Length <= 0)
                        break;

                    list.Add(line.Trim());
                }

                return list;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[SERVER-LIST] failed to update server list.");
                return null;
            }
        }

        private async Task RefreshSrvIfNeedAsync()
        {
            if (_isFixed) return;

            try
            {
                if (_serverList != null && _serverList.Count > 0) return;

                _logger?.LogDebug("server list provided by user: {0}", string.Join(",", _serverList));

                if (DateTimeOffset.Now.ToUnixTimeSeconds() - _lastServerListRefreshTime < _refreshServerListInternal) return;

                var list = await GetServerListFromEndpoint().ConfigureAwait(false);

                if (list == null || list.Count <= 0)
                    throw new Exception("Can not acquire Nacos list");

                List<string> newServerAddrList = new List<string>();

                foreach (var server in list)
                {
                    if (server.StartsWith(Nacos.Common.Constants.HTTPS, StringComparison.OrdinalIgnoreCase)
                        || server.StartsWith(Nacos.Common.Constants.HTTP, StringComparison.OrdinalIgnoreCase))
                    {
                        newServerAddrList.Add(server);
                    }
                    else
                    {
                        newServerAddrList.Add($"{Nacos.Common.Constants.HTTP}{server}");
                    }
                }

                _serversFromEndpoint = newServerAddrList;
                _lastServerListRefreshTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "failed to update server list");
            }
        }

        public string GenNextServer()
        {
            int index = Interlocked.Increment(ref _currentIndex) % GetServerList().Count;
            return GetServerList()[index];
        }

        public string GetCurrentServer()
            => GetServerList()[_currentIndex % GetServerList().Count];

        public List<string> GetServerList()
            => _serverList == null || !_serverList.Any() ? _serversFromEndpoint : _serverList;

        internal bool IsDomain() => !string.IsNullOrWhiteSpace(_nacosDomain);

        internal string GetNacosDomain() => _nacosDomain;

        private string GetFixedNameSuffix(List<string> serverIps)
        {
            StringBuilder sb = new StringBuilder(1024);
            string split = "";

            foreach (var item in serverIps)
            {
                sb.Append(split);
                var ip = Regex.Replace(item, "http(s)?://", "");
                sb.Append(ip.Replace(':', '_'));
                split = "-";
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            _refreshServerListTimer?.Dispose();
        }

        public string GetName() => _name;
    }
}
