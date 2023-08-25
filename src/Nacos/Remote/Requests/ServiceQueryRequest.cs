namespace Nacos.Remote.Requests
{
    public class ServiceQueryRequest : AbstractNamingRequest
    {
        public ServiceQueryRequest(string @namespace, string serviceName, string groupName)
            : base(@namespace, serviceName, groupName)
        {
        }

        [Newtonsoft.Json.JsonProperty("cluster")]
        public string Cluster { get; set; }

        [Newtonsoft.Json.JsonProperty("healthyOnly")]
        public bool HealthyOnly { get; set; }

        [Newtonsoft.Json.JsonProperty("udpPort")]
        public int UdpPort { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_ServiceQuery;
    }
}
