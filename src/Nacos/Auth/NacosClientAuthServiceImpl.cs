namespace Nacos.Auth
{
    using Microsoft.Extensions.Logging;
    using Nacos.Logging;
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class NacosClientAuthServiceImpl : IClientAuthService
    {
        private static readonly string LOGIN_URL = "/v1/auth/users/login";
        private static readonly int Unit = 1000;
        private static readonly int LoginTimeOut = 5000;

        private static HttpClient _httpClient = new();
        private volatile LoginIdentityContext _loginIdentityContext = new();

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

        private string contextPath;

        private List<string> _serverList;

        private readonly ILogger _logger = NacosLogManager.CreateLogger<NacosClientAuthServiceImpl>();

        public NacosClientAuthServiceImpl()
        {
        }

        // For UT only
        internal NacosClientAuthServiceImpl(HttpClient testClient)
        {
            _httpClient = testClient;
        }

        public LoginIdentityContext GetLoginIdentityContext(RequestResource resource)
            => this._loginIdentityContext;

        public async Task<bool> Login(NacosSdkOptions options)
        {
            try
            {
                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastRefreshTime < (_tokenTtl - _tokenRefreshWindow) * Unit)
                {
                    return true;
                }

                if (options.UserName.IsNullOrWhiteSpace())
                {
                    _lastRefreshTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    return true;
                }

                contextPath = options.ContextPath;
                contextPath = contextPath.StartsWith("/") ? contextPath : "/" + contextPath;

                foreach (var server in _serverList)
                {
                    var url = $"{Naming.Utils.UtilAndComs.HTTP}{server}{contextPath}{LOGIN_URL}";

                    if (server.Contains(Nacos.Common.Constants.HTTP_PREFIX))
                    {
                        url = $"{server}{contextPath}{LOGIN_URL}";
                    }

                    var dict = new Dictionary<string, string>
                    {
                        { Common.PropertyKeyConst.USERNAME, options.UserName },
                        { Common.PropertyKeyConst.PASSWORD, options.Password }
                    };

                    try
                    {
                        using var cts = new System.Threading.CancellationTokenSource();
                        cts.CancelAfter(TimeSpan.FromMilliseconds(LoginTimeOut));

                        var req = new HttpRequestMessage(HttpMethod.Post, url)
                        {
                            Content = new FormUrlEncodedContent(dict)
                        };

                        var resp = await _httpClient.SendAsync(req, cts.Token).ConfigureAwait(false);

                        if (!resp.IsSuccessStatusCode)
                        {
                            _logger.LogError("login failed: {0}", await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
                            return false;
                        }

                        var content = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var obj = Newtonsoft.Json.Linq.JObject.Parse(content);

                        if (obj.ContainsKey(Nacos.Common.Constants.ACCESS_TOKEN))
                        {
                            var accessToken = obj.Value<string>(NacosAuthLoginConstant.ACCESSTOKEN);
                            _tokenTtl = obj.Value<long>(NacosAuthLoginConstant.TOKENTTL);
                            _tokenRefreshWindow = _tokenTtl / 10;
                            _lastRefreshTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                            _loginIdentityContext.SetParameter(NacosAuthLoginConstant.ACCESSTOKEN, accessToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[NacosClientAuthServiceImpl] login http request failed, url: {0}", url);
                        return false;
                    }
                }

                return true;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public void SetServerList(List<string> serverList)
            => this._serverList = serverList;
    }
}
