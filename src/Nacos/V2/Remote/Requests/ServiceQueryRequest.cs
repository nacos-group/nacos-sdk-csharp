namespace Nacos.V2.Remote.Requests
{
    public class ServiceQueryRequest : AbstractNamingRequest
    {
        public ServiceQueryRequest(string @namespace, string serviceName, string groupName)
            : base(@namespace, serviceName, groupName)
        {
        }

        [System.Text.Json.Serialization.JsonPropertyName("cluster")]
        public string Cluster { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("healthyOnly")]
        public bool HealthyOnly { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("udpPort")]
        public int UdpPort { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_ServiceQuery;
    }
}
