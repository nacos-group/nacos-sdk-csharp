namespace Nacos.V2.Config.Http
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Common;
    using Nacos.V2.Config.Impl;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class ServerHttpAgent : IHttpAgent
    {
        private readonly ILogger _logger;
        private ServerListManager _serverListMgr;

        public ServerHttpAgent(ILogger logger, NacosSdkOptions options)
        {
            this._logger = logger;
            this._serverListMgr = new ServerListManager(logger, options);
        }

        public void Dispose()
        {
            _serverListMgr?.Dispose();
        }

        public string GetEncode() => "UTF-8";

        public string GetName() => _serverListMgr.GetName();

        public string GetNamespace() => _serverListMgr.GetNamespace();

        public string GetTenant() => _serverListMgr.GetTenant();

        public async Task<HttpResponseMessage> HttpDelete(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
            => await HttpRequest(path, headers, paramValues, encoding, readTimeoutMs, HttpMethod.Delete);

        public async Task<HttpResponseMessage> HttpGet(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
            => await HttpRequest(path, headers, paramValues, encoding, readTimeoutMs, HttpMethod.Get);

        public async Task<HttpResponseMessage> HttpPost(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
            => await HttpRequest(path, headers, paramValues, encoding, readTimeoutMs, HttpMethod.Post);

        public async Task<HttpResponseMessage> HttpRequest(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs, HttpMethod method)
        {
            long endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + readTimeoutMs;

            string currentServerAddr = _serverListMgr.GetCurrentServerAddr();
            int maxRetry = Constants.MAX_RETRY;

            var requestUrl = $"{GetUrl(currentServerAddr, path)}?{InitParams(paramValues)}";

            do
            {
                try
                {
                    using HttpClient client = new HttpClient();
                    using HttpRequestMessage reqMsg = new HttpRequestMessage(method, requestUrl);
                    foreach (var item in headers)
                    {
                        reqMsg.Headers.TryAddWithoutValidation(item.Key, item.Value);
                    }

                    var resp = await client.SendAsync(reqMsg);

                    if (IsFail(resp))
                    {
                        _logger?.LogError("[NACOS ConnectException] currentServerAddr: {0}, httpCode: {1}", currentServerAddr, resp.StatusCode);
                    }
                    else
                    {
                        _serverListMgr.UpdateCurrentServerAddr(currentServerAddr);
                        return resp;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[NACOS Exception {0}] currentServerAddr: {1}", method.Method, currentServerAddr);
                }

                maxRetry--;
                if (maxRetry < 0)
                {
                    throw new Exception(
                            $"[NACOS HTTP-{method.Method}] The maximum number of tolerable server reconnection errors has been reached");
                }

                _serverListMgr.RefreshCurrentServerAddr();
            }
            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= endTime);

            _logger?.LogError("no available server");
            throw new Exception("no available server");
        }

        public Task Start() => Task.CompletedTask;

        private string GetUrl(string serverAddr, string relativePath) => $"{serverAddr.TrimEnd('/')}/{_serverListMgr.GetContentPath()}{relativePath}";

        private string InitParams(Dictionary<string, string> dict)
        {
            var builder = new StringBuilder(1024);
            if (dict != null && dict.Any())
            {
                foreach (var item in dict)
                {
                    builder.Append($"{item.Key}={item.Value}&");
                }
            }

            return builder.ToString().TrimEnd('&');
        }

        private bool IsFail(HttpResponseMessage result)
        {
            return result.StatusCode == System.Net.HttpStatusCode.InternalServerError
                || result.StatusCode == System.Net.HttpStatusCode.BadGateway
                || result.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable;
        }
    }
}
