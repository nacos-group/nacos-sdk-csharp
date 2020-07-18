namespace Nacos.Security
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class SecurityProxy
    {
        private static readonly string LOGIN_URL = "/v1/auth/users/login";

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

        public SecurityProxy(NacosOptions options)
        {
            _username = options.UserName ?? "";
            _password = options.Password ?? "";
            contextPath = options.ContextPath;
            contextPath = contextPath.StartsWith("/") ? contextPath : "/" + contextPath;
        }

        public async Task<bool> LoginAsync(List<string> servers)
        {
            try
            {
                if ((DateTimeOffset.Now.ToUnixTimeSeconds() - _lastRefreshTime) < _tokenTtl - _tokenRefreshWindow)
                {
                    return true;
                }

                foreach (var server in servers)
                {
                    var flag = await LoginAsync(server);
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

        private async Task<bool> LoginAsync(string server)
        {
            if (!string.IsNullOrWhiteSpace(_username))
            {
                var dict = new Dictionary<string, string>
                {
                    { "username", _username },
                    { "password", _password }
                };

                var url = $"http://{server}{contextPath}{LOGIN_URL}";
                if (server.Contains(ConstValue.HTTP_PREFIX))
                {
                    url = $"{server}{contextPath}{LOGIN_URL}";
                }

                try
                {
                    using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMilliseconds(5000);

                        var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
                        {
                            Content = new FormUrlEncodedContent(dict)
                        };

                        var resp = await client.SendAsync(req);

                        if (!resp.IsSuccessStatusCode)
                        {
                            return false;
                        }

                        var content = await resp.Content.ReadAsStringAsync();

                        var obj = Newtonsoft.Json.Linq.JObject.Parse(content);

                        if (obj.ContainsKey(ConstValue.ACCESS_TOKEN))
                        {
                            _accessToken = obj.Value<string>(ConstValue.ACCESS_TOKEN);
                            _tokenTtl = obj.Value<long>(ConstValue.TOKEN_TTL);
                            _tokenRefreshWindow = _tokenTtl / 10;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
    }
}
