namespace Nacos.V2.Remote.Responses
{
    public class ServerCheckResponse : CommonResponse
    {
        [Newtonsoft.Json.JsonProperty("connectionId")]
        public string ConnectionId { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_ServerCheck;
    }
}
