namespace Nacos.V2.Security
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class SecurityProxy : ISecurityProxy
    {
        private static readonly string LOGIN_URL = "/v1/auth/users/login";
        private static readonly int Unit = 1000;

        private static HttpClient _httpClient = new HttpClient();

        private string contextPath;

        /// <summary>
        /// User's name
        /// </summary>
        private string _username;

        /// <summary>
        /// User's password
        /// </summary>
        private string _password;

        /// <summary>
        /// A token to take with when sending request to Nacos server
        /// </summary>
        private string _accessToken;

        /// <summary>
        /// TTL of token in seconds
        /// </summary>
        private long _tokenTtl;

        /// <summary>
        /// Last timestamp refresh security info from server
        /// </summary>
        private long _lastRefreshTime;

        /// <summary>
        /// time window to refresh security info in seconds
        /// </summary>
        private long _tokenRefreshWindow;

        private readonly NacosSdkOptions _options;

        private readonly ILogger _logger;

        public SecurityProxy(NacosSdkOptions options, ILogger logger)
        {
            _options = options;

            _username = _options.UserName ?? "";
            _password = _options.Password ?? "";
            contextPath = _options.ContextPath;
            contextPath = contextPath.StartsWith("/") ? contextPath : "/" + contextPath;

            _logger = logger;
        }

        // for test
        internal SecurityProxy(NacosSdkOptions options, ILogger logger, HttpMessageHandler httpMessageHandler)
        {
            _options = options;

            _username = _options.UserName ?? "";
            _password = _options.Password ?? "";
            contextPath = _options.ContextPath;
            contextPath = contextPath.StartsWith("/") ? contextPath : "/" + contextPath;

            _logger = logger;
            _httpClient = new HttpClient(httpMessageHandler);
        }

        public async Task<bool> LoginAsync(List<string> servers)
        {
            try
            {
                if ((DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastRefreshTime) < (_tokenTtl - _tokenRefreshWindow) * Unit)
                {
                    return true;
                }

                foreach (var server in servers)
                {
                    var flag = await LoginAsync(server.TrimEnd('/')).ConfigureAwait(false);
                    if (flag)
                    {
                        _lastRefreshTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        public string GetAccessToken()
        {
            return _accessToken;
        }

        public bool IsEnabled() => this._username.IsNotNullOrWhiteSpace();

        internal async Task<bool> LoginAsync(string server)
        {
            if (_username.IsNotNullOrWhiteSpace())
            {
                var dict = new Dictionary<string, string>
                {
                    { Common.PropertyKeyConst.USERNAME, _username },
                    { Common.PropertyKeyConst.PASSWORD, _password }
                };

                var url = $"{Naming.Utils.UtilAndComs.HTTP}{server}{contextPath}{LOGIN_URL}";
                if (server.Contains(Nacos.V2.Common.Constants.HTTP_PREFIX))
                {
                    url = $"{server}{contextPath}{LOGIN_URL}";
                }

                try
                {
                    var cts = new System.Threading.CancellationTokenSource();
                    cts.CancelAfter(5000);

                    var req = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new FormUrlEncodedContent(dict)
                    };

                    var resp = await _httpClient.SendAsync(req, cts.Token).ConfigureAwait(false);

                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger?.LogError("login failed: {0}", await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
                        return false;
                    }

                    var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var obj = Newtonsoft.Json.Linq.JObject.Parse(content);

                    if (obj.ContainsKey(Nacos.V2.Common.Constants.ACCESS_TOKEN))
                    {
                        _accessToken = obj.Value<string>(Nacos.V2.Common.Constants.ACCESS_TOKEN);
                        _tokenTtl = obj.Value<long>(Nacos.V2.Common.Constants.TOKEN_TTL);
                        _tokenRefreshWindow = _tokenTtl / 10;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[SecurityProxy] login http request failed, url: {0}", url);
                    return false;
                }
            }

            return true;
        }
    }
}
