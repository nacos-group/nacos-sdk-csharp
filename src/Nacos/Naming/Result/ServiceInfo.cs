namespace Nacos
{
    using System.Collections.Generic;

    public class ServiceInfo
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string name { get; set; }

        public string groupName { get; set; }

        public long cacheMillis { get; set; } = 1000L;

        public long lastRefTime { get; set; } = 0L;

        public string checksum { get; set; } = "";

        public List<Host> Hosts { get; set; }

        public bool allIPs { get; set; } = false;

        public string clusters { get; set; }

        public ServiceInfo()
        {
        }

        public ServiceInfo(string serviceName, string clusters)
        {
            this.name = serviceName;
            this.clusters = clusters;
        }

        public int IpCount()
        {
            return Hosts.Count;
        }

        public string GetKey()
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

        public string GetKeyEncoded()
        {
            return GetKey(System.Net.WebUtility.UrlEncode(name), clusters);
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
            return !string.IsNullOrEmpty(groupName)
                ? groupName + ConstValue.ServiceInfoSplitter + name
                : ConstValue.DefaultGroup + ConstValue.ServiceInfoSplitter + name;
        }
    }
}