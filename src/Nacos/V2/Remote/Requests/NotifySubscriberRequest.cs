namespace Nacos.V2.Remote.Requests
{
    public class NotifySubscriberRequest : CommonRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("groupName")]
        public string GroupName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("serviceInfo")]
        public Nacos.V2.Naming.Dtos.ServiceInfo ServiceInfo { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_NotifySubscriber;
    }
}
