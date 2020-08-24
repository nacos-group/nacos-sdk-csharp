namespace Nacos
{
    using System;
    using System.Collections.Generic;
    using Nacos.Utilities;

    public class ServiceInfo
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string name { get; set; }

        public string groupName { get; set; }

        public long cacheMillis { get; set; } = 1000L;

        // public List<GetInstanceResult> hosts { get; set; } = new ArrayList<GetInstanceResult>();
        public long lastRefTime { get; set; } = 0L;

        public string checksum { get; set; } = "";

        public List<Host> Hosts { get; set; }

        public bool allIPs { get; set; } = false;

        // public int ipCount { get; } = hosts.Count();
        public string clusters { get; set; }

        public ServiceInfo(string serviceName, string clusters)
        {
            this.name = serviceName;
            this.clusters = clusters;
        }

        public String GetKey()
        {
            return GetKey(name, clusters);
        }

        public static string GetKey(string name, string clusters)
        {
            if (!string.IsNullOrEmpty(clusters))
            {
                return name + ConstValue.ServiceInfoSplitter + clusters;
            }

            return name;
        }

        public bool Validate()
        {
            if (allIPs)
            {
                return true;
            }

            List<Host> validHosts = new List<Host>();
            foreach (Host host in Hosts)
            {
                if (!host.Healthy)
                {
                    continue;
                }

                for (int i = 0; i < host.Weight; i++)
                {
                    validHosts.Add(host);
                }
            }

            return true;
        }

        public static string GetGroupedName(string groupName, string name)
        {
            if (!string.IsNullOrEmpty(groupName))
            {
                return groupName + ConstValue.ServiceInfoSplitter + name;
            }

            return ConstValue.DefaultGroup + ConstValue.ServiceInfoSplitter + name;
        }
    }
}