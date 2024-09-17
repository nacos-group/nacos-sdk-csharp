namespace Nacos.V2.Remote.Responses
{
    public class InstanceResponse : CommonResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Naming_Instance;
    }
}
