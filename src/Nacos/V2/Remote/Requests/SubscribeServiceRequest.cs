namespace Nacos.V2.Remote.Requests
{
    public class SubscribeServiceRequest : AbstractNamingRequest
    {
        public SubscribeServiceRequest(string @namespace, string serviceName, string groupName)
            : base(@namespace, serviceName, groupName)
        {
        }

        public SubscribeServiceRequest(string @namespace, string serviceName, string groupName, string clusters, bool subscribe)
            : base(@namespace, serviceName, groupName)
        {
            this.Clisters = clusters;
            this.Subscribe = subscribe;
        }


        [Newtonsoft.Json.JsonProperty("subscribe")]
        public bool Subscribe { get; set; }

        [Newtonsoft.Json.JsonProperty("clusters")]
        public string Clisters { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_SubscribeService;
    }
}
