namespace Nacos
{
    using System;
    using System.Collections.Generic;

    public class NamingEvent : IEvent
    {
        private String serviceName;
        private String groupName;

        private String clusters;

        private List<Host> instances;

        public NamingEvent(String serviceName, List<Host> instances)
        {
            this.serviceName = serviceName;
            this.instances = instances;
        }

        public NamingEvent(String serviceName, String groupName, String clusters, List<Host> instances)
        {
            this.serviceName = serviceName;
            this.groupName = groupName;
            this.clusters = clusters;
            this.instances = instances;
        }
    }
}