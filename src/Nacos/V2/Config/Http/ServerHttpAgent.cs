namespace Nacos.V2.Config.Http
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
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
            throw new NotImplementedException();
        }

        public string GetEncode()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public string GetNamespace()
        {
            throw new NotImplementedException();
        }

        public string GetTenant()
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> HttpDelete(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> HttpGet(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
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
                    using HttpRequestMessage reqMsg = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    foreach (var item in headers)
                    {
                        reqMsg.Headers.TryAddWithoutValidation(item.Key, item.Value);
                    }

                    var resp = await client.SendAsync(reqMsg);

                    if (IsFail(resp))
                    {
                        _logger.LogError("[NACOS ConnectException] currentServerAddr: {0}, httpCode: {1}", currentServerAddr, resp.StatusCode);
                    }
                    else
                    {
                        _serverListMgr.UpdateCurrentServerAddr(currentServerAddr);
                        return resp;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[NACOS Exception httpGet] currentServerAddr: {0}", currentServerAddr);
                    throw;
                }

                maxRetry--;
                if (maxRetry < 0)
                {
                    throw new Exception(
                            "[NACOS HTTP-GET] The maximum number of tolerable server reconnection errors has been reached");
                }

                _serverListMgr.RefreshCurrentServerAddr();
            }
            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() <= endTime);

            _logger?.LogError("no available server");
            throw new Exception("no available server");
        }

        public Task<HttpResponseMessage> HttpPost(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
        {
            throw new NotImplementedException();
        }

        public Task Start()
        {
            throw new NotImplementedException();
        }

        private string GetUrl(string serverAddr, string relativePath) => $"{serverAddr}{relativePath}";

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
