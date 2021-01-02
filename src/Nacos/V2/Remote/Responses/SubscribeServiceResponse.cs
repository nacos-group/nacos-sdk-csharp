namespace Nacos.V2.Remote.Responses
{
    using System.Collections.Generic;

    public class SubscribeServiceResponse : CommonResponse
    {
        [Newtonsoft.Json.JsonProperty("serviceInfo")]
        public Nacos.V2.Naming.Dtos.ServiceInfo ServiceInfo { get; set; }

        public override string GetRemoteType() => string.Empty;
    }
}
