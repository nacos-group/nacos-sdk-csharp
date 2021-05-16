namespace Nacos.Microsoft.Extensions.Configuration
{
    using Nacos.Config;
    using Nacos.Config.Http;
    using Nacos.Security;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class MsConfigServerHttpAgent : HttpAgent, IDisposable
    {
        private static readonly HttpClient HttpClient = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan };
        private readonly NacosOptions _options;
        private readonly ServerListManager _serverListMgr;
        private readonly SecurityProxy _securityProxy;

        private readonly string _namespaceId;
        private readonly Timer _timer;
        private long _securityInfoRefreshIntervalMills = 5000;

        public MsConfigServerHttpAgent(NacosOptions options)
        {
            _options = options;
            _serverListMgr = new ServerListManager(_options);
            _namespaceId = _options.Namespace;

            _serverListMgr = new ServerListManager(_options);
            _securityProxy = new SecurityProxy(_options, null);

            _securityProxy.LoginAsync(_serverListMgr.GetServerUrls()).ConfigureAwait(false).GetAwaiter().GetResult();

            _timer = new Timer(
                async x =>
                {
                    await _securityProxy.LoginAsync(_serverListMgr.GetServerUrls()).ConfigureAwait(false);
                }, null, 0, _securityInfoRefreshIntervalMills);
        }

        public override string AbstGetName() => _serverListMgr.GetName();

        public override string AbstGetNamespace() => _serverListMgr.GetNamespace();

        public override string AbstGetTenant() => _serverListMgr.GetTenant();

        public void Dispose()
        {
            Console.WriteLine("ms config timer dispose");
            _timer?.Dispose();
        }

        public override async Task<HttpResponseMessage> ReqApiAsync(HttpMethod httpMethod, string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, CancellationToken cancellationToken)
        {
            var requestMessage = new HttpRequestMessage
            {
                Method = httpMethod
            };

            var currentServerAddr = _serverListMgr.GetCurrentServerAddr();

            var requestUrl = GetUrl(currentServerAddr, path);

            InjectSecurityInfo(requestMessage, paramValues);

            if (paramValues != null && paramValues.Any())
            {
                if (httpMethod == HttpMethod.Post)
                {
                    requestMessage.RequestUri = new Uri(requestUrl);
                    requestMessage.Content = new FormUrlEncodedContent(paramValues);
                }
                else
                {
                    var query = HttpAgentCommon.BuildQueryString(paramValues);
                    requestMessage.RequestUri = new Uri($"{requestUrl}?{query}");
                }
            }

            HttpAgentCommon.BuildHeader(requestMessage, headers);
            HttpAgentCommon.BuildSpasHeaders(requestMessage, paramValues, _options.AccessKey, _options.SecretKey);


            var responseMessage = await _httpClient.SendAsync(requestMessage, cts.Token).ConfigureAwait(false);

            if (responseMessage.StatusCode == HttpStatusCode.InternalServerError
                || responseMessage.StatusCode == HttpStatusCode.BadGateway
                || responseMessage.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                Trace.TraceError("[NACOS ConnectException] currentServerAddr: {0}, httpCode: {1}", _serverListMgr.GetCurrentServerAddr(), responseMessage.StatusCode);
            }
            else
            {
                _serverListMgr.UpdateCurrentServerAddr(currentServerAddr);
                return responseMessage;
            }

            throw new HttpRequestException($"no available server, currentServerAddr : {currentServerAddr}");
        }

        private void InjectSecurityInfo(HttpRequestMessage requestMessage, Dictionary<string, string> paramValues)
        {
            if (!string.IsNullOrWhiteSpace(_securityProxy.GetAccessToken()))
            {
                if (!paramValues.ContainsKey(ConstValue.ACCESS_TOKEN))
                {
                    paramValues.Add(ConstValue.ACCESS_TOKEN, _securityProxy.GetAccessToken());
                }
            }

            if (!string.IsNullOrWhiteSpace(_namespaceId) && paramValues != null && !paramValues.ContainsKey("tenant"))
            {
                requestMessage.Headers.TryAddWithoutValidation("tenant", _namespaceId);
            }
        }

        private string GetUrl(string serverAddr, string relativePath)
        {
            return $"{serverAddr}{relativePath}";
        }
    }
}
