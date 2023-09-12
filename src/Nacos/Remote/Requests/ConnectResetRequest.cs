namespace Nacos.Remote.Requests
{
    public class ConnectResetRequest : CommonRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("serverIp")]
        public string ServerIp { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("serverPort")]
        public string ServerPort { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_ConnectReset;
    }
}
