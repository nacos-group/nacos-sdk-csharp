namespace Nacos.Remote
{
    using System.Linq;

    public abstract class CommonRequest
    {
        [Newtonsoft.Json.JsonProperty("headers")]
        public System.Collections.Generic.Dictionary<string, string> Headers { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

        [Newtonsoft.Json.JsonProperty("requestId")]
        public string RequestId { get; set; }

        public void PutHeader(string key, string value)
        {
            Headers[key] = value;
        }

        public void PutAllHeader(System.Collections.Generic.Dictionary<string, string> headers)
        {
            if (headers == null || !headers.Any()) return;

            foreach (var item in headers) Headers[item.Key] = item.Value;
        }

        public string GetHeader(string key, string defaultValue)
        {
            return !Headers.TryGetValue(key, out var value)
                ? defaultValue
                : value;
        }

        public abstract string GetRemoteType();
    }
}
