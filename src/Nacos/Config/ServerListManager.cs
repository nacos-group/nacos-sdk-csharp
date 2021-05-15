﻿namespace Nacos.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class ServerListManager : IDisposable
    {
        public const string FIXED_NAME = "fixed";
        private const string HTTP = "http";
        private const string HTTPS = "https";

        private string _name = "";
        private string _namespace = "";
        private string _tenant = "";
        private int _endpointPort = 8080;
        private readonly string _contentPath;
        private readonly string _defaultNodesPath;

        private readonly bool _isFixed = false;
        private List<string> _serverUrls;
        private string _currentServerAddr;

        private string _addressServerUrl;

        private Timer t;

        public ServerListManager(NacosOptions options)
        {
            _serverUrls = new List<string>();
            _contentPath = options.ContextPath;
            _defaultNodesPath = options.ClusterName;
            var serverAddresses = options.ServerAddresses;
            var @namespace = options.Namespace;

            if (serverAddresses != null && serverAddresses.Any())
            {
                _isFixed = true;
                foreach (var item in serverAddresses)
                {
                    // here only trust the input server addresses of user
                    _serverUrls.Add(item.TrimEnd('/'));
                }

                if (string.IsNullOrWhiteSpace(@namespace))
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
                if (string.IsNullOrWhiteSpace(options.EndPoint))
                {
                    throw new Nacos.Exceptions.NacosException(ConstValue.CLIENT_INVALID_PARAM, "endpoint is blank");
                }

                _isFixed = false;

                if (string.IsNullOrWhiteSpace(@namespace))
                {
                    _name = options.EndPoint;
                    _addressServerUrl = $"http://{options.EndPoint}:{_endpointPort}/{_contentPath}/{_defaultNodesPath}";
                }
                else
                {
                    _namespace = @namespace;
                    _tenant = $"{options.EndPoint}-{@namespace}";
                    _name = $"{FIXED_NAME}-{GetFixedNameSuffix(_serverUrls)}-{@namespace}";

                    _addressServerUrl = $"http://{options.EndPoint}:{_endpointPort}/{_contentPath}/{_defaultNodesPath}?namespace={@namespace}";
                }

                t = new Timer(
                    async x =>
                    {
                        await RefreshSrvAsync().ConfigureAwait(false);
                    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

                RefreshSrvAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            System.Diagnostics.Trace.WriteLine(_isFixed);
        }

        private async Task RefreshSrvAsync()
        {
            try
            {
                if (_serverUrls != null && _serverUrls.Count > 0) return;

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
            catch
            {
            }
        }

        private async Task<List<string>> GetServerListFromEndpointAsync()
        {
            var list = new List<string>();
            var result = new List<string>();
            try
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromMilliseconds(3000);

                    var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, _addressServerUrl);

                    var resp = await client.SendAsync(req).ConfigureAwait(false);

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
                            if (!string.IsNullOrWhiteSpace(item))
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
                        return null;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public List<string> GetServerUrls()
        {
            return _serverUrls;
        }

        public string GetCurrentServerAddr()
        {
            if (string.IsNullOrWhiteSpace(_currentServerAddr))
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

        public void UpdateCurrentServerAddr(string currentServerAddr)
        {
            _currentServerAddr = currentServerAddr;
        }

        public string GetName() => _name;

        public string GetNamespace() => _namespace;

        public string GetTenant() => _tenant;

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
            t?.Dispose();
        }
    }
}
