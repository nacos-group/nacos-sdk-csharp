namespace Nacos.V2.Remote.GRpc
{
    public class RequestMeta
    {
        public string ConnectionId { get; set; } = "";

        public string ClientIp { get; set; } = "";

        public int ClientPort { get; set; }

        public string ClientVersion { get; set; } = "";

        public string Type { get; set; } = "";

        public System.Collections.Generic.Dictionary<string, string> Labels { get; set; } = new System.Collections.Generic.Dictionary<string, string>();
    }
}
