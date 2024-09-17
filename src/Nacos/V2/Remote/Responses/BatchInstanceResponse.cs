namespace Nacos.V2.Remote.Responses
{
    public class BatchInstanceResponse : CommonResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Naming_SubscribeService;
    }
}
