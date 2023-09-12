namespace Nacos.Remote.Requests
{
    using Nacos.Naming.Dtos;
    using Nacos.Remote;
    using System.Collections.Generic;

    public class BatchInstanceRequest : AbstractNamingRequest
    {
        public BatchInstanceRequest(string @namespace, string serviceName, string groupName, string type, List<Instance> instances)
            : base(@namespace, serviceName, groupName)
        {
            Type = type;
            Instances = instances;
        }

        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("instances")]
        public List<Instance> Instances { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_SubscribeService;
    }
}
