namespace Nacos.Naming.Dtos
{
    using Nacos.Common;
    using System;
    using System.Collections.Generic;

    public class ServiceInfo
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public string name { get; set; }

        public string groupName { get; set; }

        public long cacheMillis { get; set; } = 1000L;

        public long lastRefTime { get; set; } = 0L;

        public string checksum { get; set; } = "";

        public List<Instance> hosts { get; set; } = new List<Instance>();

        public bool allIPs { get; set; } = false;

        public string clusters { get; set; }

        public ServiceInfo()
        {
        }

        public ServiceInfo(string key)
        {
            int maxIndex = 2;
            int clusterIndex = 2;
            int serviceNameIndex = 1;
            int groupIndex = 0;

            var keys = key.Split(new string[] { Constants.SERVICE_INFO_SPLITER }, StringSplitOptions.RemoveEmptyEntries);
            if (keys.Length >= maxIndex + 1)
            {
                this.groupName = keys[groupIndex];
                this.name = keys[serviceNameIndex];
                this.clusters = keys[clusterIndex];
            }
            else if (keys.Length == maxIndex)
            {
                this.groupName = keys[groupIndex];
                this.name = keys[serviceNameIndex];
            }
            else
            {
                // defensive programming
                throw new ArgumentException("Cann't parse out 'groupName',but it must not be null!");
            }
        }

        public ServiceInfo(string serviceName, string clusters)
        {
            this.name = serviceName;
            this.clusters = clusters;
        }

        public int IpCount() => hosts.Count;

        public string GetKey() => GetKey(GetGroupedServiceName(), clusters);

        public static string GetKey(string name, string clusters)
            => !string.IsNullOrEmpty(clusters) ? name + Constants.SERVICE_INFO_SPLITER + clusters : name;

        public string GetKeyEncoded() => GetKey(System.Net.WebUtility.UrlEncode(GetGroupedServiceName()), clusters);

        public bool Validate()
        {
            if (allIPs) return true;

            var validHosts = new List<Instance>();
            foreach (var host in hosts)
            {
                if (!host.Healthy) continue;

                for (int i = 0; i < host.Weight; i++) validHosts.Add(host);
            }

            return true;
        }

        private string GetGroupedServiceName()
            => !string.IsNullOrWhiteSpace(groupName) && this.name.IndexOf(Constants.SERVICE_INFO_SPLITER) == -1
                ? groupName + Constants.SERVICE_INFO_SPLITER + this.name
                : this.name;

        public static ServiceInfo FromKey(string key)
        {
            var serviceInfo = new ServiceInfo();
            int maxSegCount = 3;
            string[] segs = key.Split(new string[] { Constants.SERVICE_INFO_SPLITER }, StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length == maxSegCount - 1)
            {
                serviceInfo.groupName = segs[0];
                serviceInfo.name = segs[1];
            }
            else if (segs.Length == maxSegCount)
            {
                serviceInfo.groupName = segs[0];
                serviceInfo.name = segs[1];
                serviceInfo.clusters = segs[2];
            }

            return serviceInfo;
        }
    }
}
