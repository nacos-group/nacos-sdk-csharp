namespace Nacos.V2.Remote.Responses
{
    public class InstanceResponse : CommonResponse
    {
        [Newtonsoft.Json.JsonProperty("type")]
        public string Type { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Naming_Instance;
    }
}
