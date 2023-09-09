﻿namespace Nacos.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Nacos.Utils;
    using Nacos.Exceptions;
    using Nacos;
    using Nacos.Config.Abst;
    using Nacos.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Common;

    public class ServerListManager : IServerListManager
    {
        private readonly ILogger _logger = NacosLogManager.CreateLogger<ServerListManager>();

        private static HttpClient _httpClient = new();

        public const string FIXED_NAME = "fixed";
        private const string HTTP = "http";
        private const string HTTPS = "https";
        private const string DefaultNodesPath = "serverlist";
        private const int DEFAULT_TIMEOUT = 5000;

        private string _name = "";
        private string _namespace = "";
        private string _tenant = "";
        private int _endpointPort = 8080;

        private List<string> _serverUrls;
        private string _currentServerAddr;
        private string _addressServerUrl;

        private Timer _refreshSvcListTimer;

        private readonly string _contentPath;
        private readonly string _defaultNodesPath;
        private readonly bool _isFixed = false;
        private readonly ILogger _logger = NacosLogManager.CreateLogger<ServerListManager>();
        private readonly NacosSdkOptions _options;

        public ServerListManager(IOptions<NacosSdkOptions> optionsAccs)
        {
            _options = optionsAccs.Value;
            _serverUrls = new List<string>();
            _contentPath = _options.ContextPath;
            _defaultNodesPath = DefaultNodesPath;
            var @namespace = _options.Namespace;

            if (_options.ServerAddresses != null && _options.ServerAddresses.Any())
            {
                _isFixed = true;
                foreach (var item in _options.ServerAddresses)
                {
                    // here only trust the input server addresses of user
                    _serverUrls.Add(item.TrimEnd('/'));
                }

                if (@namespace.IsNullOrWhiteSpace())
                {
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverUrls)}";
                }
                else
                {
                    _namespace = @namespace;
                    _tenant = @namespace;
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverUrls)}-{@namespace}";
                }
            }
            else
            {
                if (_options.EndPoint.IsNullOrWhiteSpace())
                {
                    throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "endpoint is blank");
                }

                _isFixed = false;

                var endpoint = _options.EndPoint.IndexOf(':') == -1 ? $"{_options.EndPoint}:{_endpointPort}" : _options.EndPoint;
                if (@namespace.IsNullOrWhiteSpace())
                {
                    _name = _options.EndPoint;
                    _addressServerUrl = $"http://{endpoint}/{_contentPath}/{_defaultNodesPath}";
                }
                else
                {
                    _namespace = @namespace;
                    _tenant = $"{_options.EndPoint}-{@namespace}";
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverUrls)}-{@namespace}";

                    _addressServerUrl = $"http://{endpoint}/{_contentPath}/{_defaultNodesPath}?namespace={@namespace}";
                }

                _refreshSvcListTimer = new Timer(
                    async x =>
                    {
                        await RefreshSrvAsync().ConfigureAwait(false);
                    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

                RefreshSrvAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private async Task RefreshSrvAsync()
        {
            if (_isFixed) return;

            try
            {
                if (_serverUrls != null && _serverUrls.Count > 0) return;

                _logger?.LogWarning("[update-serverlist] current serverlist from address server is empty!!!");

                var list = await GetServerListFromEndpointAsync().ConfigureAwait(false);

                if (list == null || list.Count <= 0)
                {
                    throw new Exception("Can not acquire Nacos list");
                }

                List<string> newServerAddrList = new List<string>();

                foreach (var server in list)
                {
                    if (server.StartsWith(HTTP, StringComparison.OrdinalIgnoreCase)
                        || server.StartsWith(HTTPS, StringComparison.OrdinalIgnoreCase))
                    {
                        newServerAddrList.Add(server);
                    }
                    else
                    {
                        newServerAddrList.Add($"{HTTP}://{server}");
                    }
                }

                _serverUrls = new List<string>(newServerAddrList);

                Random random = new Random();
                int index = random.Next(0, _serverUrls.Count);
                _currentServerAddr = _serverUrls[index];
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[{0}][update-serverlist] failed to update serverlist from address server!", _name);
            }
        }

        private async Task<List<string>> GetServerListFromEndpointAsync()
        {
            var list = new List<string>();
            var result = new List<string>();
            try
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(DEFAULT_TIMEOUT));

                var req = new HttpRequestMessage(HttpMethod.Get, _addressServerUrl);

                var resp = await _httpClient.SendAsync(req, cts.Token).ConfigureAwait(false);

                if (resp.IsSuccessStatusCode)
                {
                    var str = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    using (StringReader sr = new StringReader(str))
                    {
                        while (true)
                        {
                            var line = await sr.ReadLineAsync().ConfigureAwait(false);
                            if (line == null || line.Length <= 0)
                                break;

                            list.Add(line.Trim());
                        }
                    }

                    foreach (var item in list)
                    {
                        if (item.IsNotNullOrWhiteSpace())
                        {
                            var ipPort = item.Trim().Split(':');
                            var ip = ipPort[0].Trim();
                            if (ipPort.Length == 1)
                            {
                                result.Add($"{ip}:8848");
                            }
                            else
                            {
                                result.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    _logger?.LogWarning("get serverlist fail,url: {0}", _addressServerUrl);
                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[init-serverlist] fail to get NACOS-server serverlist! env: {0}, url: {1}", _name, _addressServerUrl);
                return null;
            }
        }

        public List<string> GetServerUrls() => _serverUrls;

        public string GetCurrentServerAddr()
        {
            if (_currentServerAddr.IsNullOrWhiteSpace())
            {
                Random random = new Random();
                int index = random.Next(0, _serverUrls.Count);
                _currentServerAddr = _serverUrls[index];
            }

            return _currentServerAddr;
        }

        public void RefreshCurrentServerAddr()
        {
            Random random = new Random();
            int index = random.Next(0, _serverUrls.Count);
            _currentServerAddr = _serverUrls[index];
        }

        public void UpdateCurrentServerAddr(string currentServerAddr) => _currentServerAddr = currentServerAddr;

        public string GetName() => _name;

        public string GetNamespace() => _namespace;

        public string GetTenant() => _tenant;

        public string GetContentPath() => _contentPath;

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
            _refreshSvcListTimer?.Dispose();
        }

        public string GetNextServerAddr()
        {
            RefreshCurrentServerAddr();
            return _currentServerAddr;
        }
    }
}
