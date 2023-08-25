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

        /* 项目“Nacos (netstandard2.0)”的未合并的更改
        在此之前:
                public Nacos.Naming.Dtos.ServiceInfo ServiceInfo { get; set; }
        在此之后:
                public ServiceInfo ServiceInfo { get; set; }
        */
        public Naming.Dtos.ServiceInfo ServiceInfo { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_NotifySubscriber;
    }
}
