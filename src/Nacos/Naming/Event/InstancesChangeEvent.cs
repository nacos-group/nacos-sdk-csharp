namespace Nacos.Naming.Event
{
    using Nacos.Naming;
    using Nacos.Naming.Dtos;
    using System.Collections.Generic;

    public class InstancesChangeEvent : IEvent
    {
        [System.Text.Json.Serialization.JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("groupName")]
        public string GroupName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("clusters")]
        public string Clusters { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("hosts")]
        public List<Instance> Hosts { get; set; }

        public InstancesChangeEvent(string serviceName, string groupName, string clusters, List<Instance> hosts)
        {
            ServiceName = serviceName;
            GroupName = groupName;
            Clusters = clusters;
            Hosts = hosts;
        }
    }
}
