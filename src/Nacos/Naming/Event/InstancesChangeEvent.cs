namespace Nacos.Naming.Event
{
    using Nacos.Naming;
    using Nacos.Naming.Dtos;
    using System.Collections.Generic;

    public class InstancesChangeEvent : IEvent
    {
        [Newtonsoft.Json.JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [Newtonsoft.Json.JsonProperty("groupName")]
        public string GroupName { get; set; }

        [Newtonsoft.Json.JsonProperty("clusters")]
        public string Clusters { get; set; }

        [Newtonsoft.Json.JsonProperty("hosts")]
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
