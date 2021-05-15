namespace Nacos.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class HttpClientFactoryUtil
    {
        public static async Task<HttpResponseMessage> DoRequestAsync(this IHttpClientFactory factory, HttpMethod method, string url, string param = "", int timeOut = 8, Dictionary<string, string> headers = null, string secretKey = "")
        {
            var client = factory.CreateClient(ConstValue.ClientName);
            client.Timeout = TimeSpan.FromSeconds(timeOut);

            var requestUrl = string.IsNullOrWhiteSpace(param) ? url : $"{url}?{param}";

            var requestMessage = new HttpRequestMessage(method, requestUrl);

            BuildHeader(requestMessage, headers);

            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                BuildSignHeader(requestMessage, param, secretKey);
            }

            var responseMessage = await client.SendAsync(requestMessage).ConfigureAwait(false);
            return responseMessage;
        }

        public static void BuildHeader(HttpRequestMessage requestMessage, Dictionary<string, string> headers)
        {
            requestMessage.Headers.Clear();

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    requestMessage.Headers.Add(item.Key, item.Value);
                }
            }

            requestMessage.Headers.TryAddWithoutValidation("Client-Version", ConstValue.ClientVersion);
            requestMessage.Headers.TryAddWithoutValidation("User-Agent", ConstValue.ClientVersion);
            requestMessage.Headers.TryAddWithoutValidation("RequestId", Guid.NewGuid().ToString());
            requestMessage.Headers.TryAddWithoutValidation("Request-Module", ConstValue.RequestModule);
            requestMessage.Headers.TryAddWithoutValidation("exConfigInfo", "true");
        }

        public static void BuildSignHeader(HttpRequestMessage requestMessage, string param, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(param)) return;

            var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            requestMessage.Headers.TryAddWithoutValidation("timeStamp", timeStamp);

            var dict = GetDictFromParam(param);

            var resource = dict.ContainsKey("tenant") && dict["tenant"].Length > 0
                ? string.Concat(dict["tenant"], "+", dict["group"])
                : dict["group"];

            var signature = string.IsNullOrWhiteSpace(resource)
                ? HashUtil.GetHMACSHA1(timeStamp, secretKey)
                : HashUtil.GetHMACSHA1($"{resource}+{timeStamp}", secretKey);

            requestMessage.Headers.TryAddWithoutValidation("Spas-Signature", signature);
        }

        private static Dictionary<string, string> GetDictFromParam(string param)
        {
            var dict = new Dictionary<string, string>();
            var arr = param.Split('&');
            foreach (var item in arr)
            {
                var kv = item.Split('=');
                dict.Add(kv[0], kv[1]);
            }

            return dict;
        }
    }
}
