namespace Nacos.V2.Remote.Requests
{
    public class ConnectResetRequest : CommonRequest
    {
        [Newtonsoft.Json.JsonProperty("serverIp")]
        public string ServerIp { get; set; }

        [Newtonsoft.Json.JsonProperty("serverPort")]
        public string ServerPort { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_ConnectReset;
    }
}
