namespace Nacos.Remote.Requests
{
    public abstract class AbstractNamingRequest : CommonRequest
    {
        [Newtonsoft.Json.JsonProperty("namespace")]
        public string Namespace { get; set; }

        [Newtonsoft.Json.JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [Newtonsoft.Json.JsonProperty("groupName")]
        public string GroupName { get; set; }

        public AbstractNamingRequest(string @namespace, string serviceName, string groupName)
        {
            this.Namespace = @namespace;
            this.ServiceName = serviceName;
            this.GroupName = groupName;
        }
    }
}
