namespace Nacos
{
    using System;
    using System.Collections.Generic;

    public class NamingEvent : IEvent
    {
        public string ServiceName { get; set; }

        public string GroupName { get; set; }

        public string Clusters { get; set; }

        public List<Host> Instances { get; set; }

        public NamingEvent(String serviceName, List<Host> instances)
        {
            this.ServiceName = serviceName;
            this.Instances = instances;
        }

        public NamingEvent(String serviceName, String groupName, String clusters, List<Host> instances)
        {
            this.ServiceName = serviceName;
            this.GroupName = groupName;
            this.Clusters = clusters;
            this.Instances = instances;
        }
    }
}