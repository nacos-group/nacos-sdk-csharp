namespace Nacos.V2.Remote.Responses
{
    using System.Collections.Generic;

    public class ServiceListResponse : CommonResponse
    {
        [Newtonsoft.Json.JsonProperty("count")]
        public int Count { get; set; }

        [Newtonsoft.Json.JsonProperty("serviceNames")]
        public List<string> ServiceNames { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Naming_ServiceList;
    }
}
