namespace Nacos.V2.Naming.Event
{
    using Nacos.V2.Naming.Dtos;
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
            this.ServiceName = serviceName;
            this.GroupName = groupName;
            this.Clusters = clusters;
            this.Hosts = hosts;
        }
    }
}
