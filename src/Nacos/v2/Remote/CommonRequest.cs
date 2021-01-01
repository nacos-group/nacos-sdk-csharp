namespace Nacos.Remote
{
    public abstract class CommonRequest
    {
        [Newtonsoft.Json.JsonProperty("headers")]
        public System.Collections.Generic.Dictionary<string, string> Headers { get; set; } = new System.Collections.Generic.Dictionary<string, string>();

        [Newtonsoft.Json.JsonProperty("requestId")]
        public string RequestId { get; set; }

        public void PutHeader(string key, string value)
        {
            this.Headers[key] = value;
        }

        public string GetHeader(string key, string defaultValue)
        {
            return !Headers.TryGetValue(key, out var value)
                ? defaultValue
                : value;
        }

        public abstract string GetGrpcType();
    }
}
