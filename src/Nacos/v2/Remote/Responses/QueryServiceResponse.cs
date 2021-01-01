namespace Nacos.Remote.Responses
{
    public class QueryServiceResponse : CommonResponse
    {
        [Newtonsoft.Json.JsonProperty("serviceInfo")]
        public Nacos.Naming.Dtos.ServiceInfo ServiceInfo { get; set; }

        public override string GetRemoteType() => "";
    }
}
