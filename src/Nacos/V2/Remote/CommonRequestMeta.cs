namespace Nacos.V2.Remote
{
    public class CommonRequestMeta
    {
        [System.Text.Json.Serialization.JsonPropertyName("connectionId")]
        public string ConnectionId { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("clientIp")]
        public string ClientIp { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("clientPort")]
        public int ClientPort { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("clientVersion")]
        public string ClientVersion { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [System.Text.Json.Serialization.JsonPropertyName("labels")]
        public System.Collections.Generic.Dictionary<string, string> Labels { get; set; } = new System.Collections.Generic.Dictionary<string, string>();
    }
}
