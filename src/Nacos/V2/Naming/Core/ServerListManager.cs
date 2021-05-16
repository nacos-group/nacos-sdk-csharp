namespace Nacos.V2.Naming.Core
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Remote;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class ServerListManager : IServerListFactory, IDisposable
    {
        private static HttpClient _httpClient = new HttpClient();

        private readonly ILogger _logger;

        private long _refreshServerListInternal = 30000;

        private int _currentIndex = 0;

        private List<string> _serversFromEndpoint = new List<string>();

        private List<string> _serverList = new List<string>();

        private Timer _refreshServerListTimer;

        private string _endpoint;

        private string _nacosDomain;

        private long _lastServerListRefreshTime = 0L;

        public ServerListManager(ILogger logger, NacosSdkOptions options)
        {
            this._logger = logger;
            InitServerAddr(options);
        }

        private void InitServerAddr(NacosSdkOptions options)
        {
            this._endpoint = options.EndPoint;

            if (!string.IsNullOrWhiteSpace(_endpoint))
            {
                this._serversFromEndpoint = GetServerListFromEndpoint()
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                this._refreshServerListTimer = new Timer(
                    async x =>
                    {
                        await RefreshSrvIfNeedAsync().ConfigureAwait(false);
                    }, null, 0, _refreshServerListInternal);
            }
            else
            {
                var serverlist = options.ServerAddresses;
                this._serverList.AddRange(serverlist);
                if (this._serverList.Count == 1)
                {
                    this._nacosDomain = serverlist[0];
                }
            }
        }

        private async Task<List<string>> GetServerListFromEndpoint()
        {
            var list = new List<string>();
            try
            {
                var url = $"http://{_endpoint}/nacos/serverlist";

                var header = Utils.NamingHttpUtil.BuildHeader();

                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(5000));

                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
                foreach (var item in header) req.Headers.TryAddWithoutValidation(item.Key, item.Value);

                var resp = await _httpClient.SendAsync(req).ConfigureAwait(false);

                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception($"Error while requesting: {url} . Server returned: {resp.StatusCode}");
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
                _logger?.LogError(ex, "GetServerListFromEndpoint error.");
                return null;
            }
        }

        private async Task RefreshSrvIfNeedAsync()
        {
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
                    if (server.StartsWith(ConstValue.HTTPS, StringComparison.OrdinalIgnoreCase)
                        || server.StartsWith(ConstValue.HTTP, StringComparison.OrdinalIgnoreCase))
                    {
                        newServerAddrList.Add(server);
                    }
                    else
                    {
                        newServerAddrList.Add($"{ConstValue.HTTP}{server}");
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

        public void Dispose()
        {
            _refreshServerListTimer?.Dispose();
        }
    }
}
