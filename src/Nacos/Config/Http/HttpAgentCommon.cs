namespace Nacos.Config.Http
{
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    public static class HttpAgentCommon
    {
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
        }

        public static void BuildSpasHeaders(HttpRequestMessage requestMessage, Dictionary<string, string> paramValues, string accessKey = "", string secretKey = "")
        {
            if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
            {
                requestMessage.Headers.TryAddWithoutValidation("Spas-AccessKey", accessKey);
                BuildSignHeaders(requestMessage, paramValues, secretKey);
            }
        }

        public static void BuildSignHeaders(HttpRequestMessage requestMessage, Dictionary<string, string> paramValues, string secretKey)
        {
            var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            requestMessage.Headers.TryAddWithoutValidation("timeStamp", timeStamp);

            var resource = paramValues.ContainsKey("tenant") && paramValues["tenant"].Length > 0
                ? string.Concat(paramValues["tenant"], "+", paramValues["group"])
                : paramValues["group"];

            var signature = string.IsNullOrWhiteSpace(resource)
                ? HashUtil.GetHMACSHA1(timeStamp, secretKey)
                : HashUtil.GetHMACSHA1($"{resource}+{timeStamp}", secretKey);

            requestMessage.Headers.TryAddWithoutValidation("Spas-Signature", signature);
        }

        public static string BuildQueryString(Dictionary<string, string> paramValues)
        {
            var query = new System.Text.StringBuilder(1024);

            foreach (var item in paramValues)
            {
                query.Append($"{item.Key}={item.Value}&");
            }

            return query.ToString().TrimEnd('&');
        }
    }
}
