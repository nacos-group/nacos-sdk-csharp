namespace Nacos.V2.Naming.Dtos
{
    using Nacos.V2.Common;
    using System;
    using System.Collections.Generic;

    public class ServiceInfo
    {
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("groupName")]
        public string GroupName { get; set; }

        [Newtonsoft.Json.JsonProperty("cacheMillis")]
        public long CacheMillis { get; set; } = 1000L;

        [Newtonsoft.Json.JsonProperty("lastRefTime")]
        public long LastRefTime { get; set; } = 0L;

        [Newtonsoft.Json.JsonProperty("checksum")]
        public string Checksum { get; set; } = "";

        [Newtonsoft.Json.JsonProperty("hosts")]
        public List<Instance> Hosts { get; set; } = new List<Instance>();

        [Newtonsoft.Json.JsonProperty("metallIPsadata")]
        public bool AllIPs { get; set; } = false;

        [Newtonsoft.Json.JsonProperty("clusters")]
        public string Clusters { get; set; }

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
                this.GroupName = keys[groupIndex];
                this.Name = keys[serviceNameIndex];
                this.Clusters = keys[clusterIndex];
            }
            else if (keys.Length == maxIndex)
            {
                this.GroupName = keys[groupIndex];
                this.Name = keys[serviceNameIndex];
            }
            else
            {
                // defensive programming
                throw new ArgumentException("Cann't parse out 'groupName',but it must not be null!");
            }
        }

        public ServiceInfo(string serviceName, string clusters)
        {
            this.Name = serviceName;
            this.Clusters = clusters;
        }

        public int IpCount() => Hosts.Count;

        public string GetKey() => GetKey(GetGroupedServiceName(), Clusters);

        public static string GetKey(string name, string clusters)
            => !string.IsNullOrEmpty(clusters) ? name + Constants.SERVICE_INFO_SPLITER + clusters : name;


        [Newtonsoft.Json.JsonIgnore]
        public string JsonFromServer { get; set; }

        public string GetKeyEncoded() => GetKey(System.Net.WebUtility.UrlEncode(GetGroupedServiceName()), Clusters);

        public bool Validate()
        {
            if (AllIPs) return true;

            var validHosts = new List<Instance>();
            foreach (var host in Hosts)
            {
                if (!host.Healthy) continue;

                for (int i = 0; i < host.Weight; i++) validHosts.Add(host);
            }

            return true;
        }

        private string GetGroupedServiceName()
            => !string.IsNullOrWhiteSpace(GroupName) && this.Name.IndexOf(Constants.SERVICE_INFO_SPLITER) == -1
                ? GroupName + Constants.SERVICE_INFO_SPLITER + this.Name
                : this.Name;

        public static ServiceInfo FromKey(string key)
        {
            var serviceInfo = new ServiceInfo();
            int maxSegCount = 3;
            string[] segs = key.Split(new string[] { Constants.SERVICE_INFO_SPLITER }, StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length == maxSegCount - 1)
            {
                serviceInfo.GroupName = segs[0];
                serviceInfo.Name = segs[1];
            }
            else if (segs.Length == maxSegCount)
            {
                serviceInfo.GroupName = segs[0];
                serviceInfo.Name = segs[1];
                serviceInfo.Clusters = segs[2];
            }

            return serviceInfo;
        }

        public override string ToString() => GetKey();
    }
}
