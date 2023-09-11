namespace Nacos.Remote.Responses
{
    public class ServerCheckResponse : CommonResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("connectionId")]
        public string ConnectionId { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_ServerCheck;
    }
}
