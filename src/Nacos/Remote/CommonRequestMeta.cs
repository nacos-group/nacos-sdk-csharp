namespace Nacos.Remote
{
    public class CommonRequestMeta
    {
        [Newtonsoft.Json.JsonProperty("connectionId")]
        public string ConnectionId { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("clientIp")]
        public string ClientIp { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("clientPort")]
        public int ClientPort { get; set; }

        [Newtonsoft.Json.JsonProperty("clientVersion")]
        public string ClientVersion { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("type")]
        public string Type { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("labels")]
        public System.Collections.Generic.Dictionary<string, string> Labels { get; set; } = new System.Collections.Generic.Dictionary<string, string>();
    }
}
