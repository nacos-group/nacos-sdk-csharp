namespace Nacos.V2.Remote.Responses
{
    public class QueryServiceResponse : CommonResponse
    {
        [Newtonsoft.Json.JsonProperty("serviceInfo")]
        public Nacos.V2.Naming.Dtos.ServiceInfo ServiceInfo { get; set; }

        public override string GetRemoteType() => "";
    }
}
