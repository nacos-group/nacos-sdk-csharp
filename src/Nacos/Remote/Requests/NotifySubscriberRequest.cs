namespace Nacos.Remote.Requests
{
    public class NotifySubscriberRequest : CommonRequest
    {
        [Newtonsoft.Json.JsonProperty("namespace")]
        public string Namespace { get; set; }

        [Newtonsoft.Json.JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [Newtonsoft.Json.JsonProperty("groupName")]
        public string GroupName { get; set; }

        [Newtonsoft.Json.JsonProperty("serviceInfo")]
        public Naming.Dtos.ServiceInfo ServiceInfo { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_NotifySubscriber;
    }
}
