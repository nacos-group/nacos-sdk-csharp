namespace Nacos.AspNetCore
{
    using System.Collections.Generic;

    public class NacosAspNetCoreOptions
    {
        /// <summary>
        /// nacos server addresses.
        /// </summary>
        /// <example>
        /// http://10.1.12.123:8848,https://10.1.12.124:8848
        /// </example>
        public List<string> ServerAddresses { get; set; }

        /// <summary>
        /// default timeout, unit is Milliseconds.
        /// </summary>
        public int DefaultTimeOut { get; set; } = 15000;

        /// <summary>
        /// default namespace
        /// </summary>
        public string Namespace { get; set; } = "";

        /// <summary>
        /// the name of the service.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// the name of the cluster.
        /// </summary>
        /// <value>The name of the cluster.</value>
        public string ClusterName { get; set; }

        /// <summary>
        /// the name of the group.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// the weight of this instance.
        /// </summary>
        public double Weight { get; set; } = 100;

        /// <summary>
        /// the ip of this instance
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Select an IP that matches the prefix as the service registration IP
        /// like the config of spring.cloud.inetutils.preferred-networks
        /// </summary>
        public string PreferredNetworks { get; set; }

        /// <summary>
        /// the port of this instance
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// if you just want to subscribe, but don't want to register your service, set it to false.
        /// </summary>
        public bool RegisterEnabled { get; set; } = true;

        /// <summary>
        /// the metadata of this instance
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Load Balance Strategy
        /// </summary>
        public string LBStrategy { get; set; } = LBStrategyName.WeightRandom.ToString();
    }
}
