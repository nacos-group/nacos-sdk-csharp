namespace Nacos.AspNetCore.V2
{
    using Nacos.V2;
    using System.Collections.Generic;

    public class NacosAspNetOptions : NacosSdkOptions
    {
        /// <summary>
        /// the name of the service.
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// the name of the group.
        /// </summary>
        public string GroupName { get; set; } = Nacos.V2.Common.Constants.DEFAULT_GROUP;

        /// <summary>
        /// the name of the cluster.
        /// </summary>
        /// <value>The name of the cluster.</value>
        public string ClusterName { get; set; } = Nacos.V2.Common.Constants.DEFAULT_CLUSTER_NAME;

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
        /// the weight of this instance.
        /// </summary>
        public double Weight { get; set; } = 100;

        /// <summary>
        /// if you just want to subscribe, but don't want to register your service, set it to false.
        /// </summary>
        public bool RegisterEnabled { get; set; } = true;

        /// <summary>
        /// the metadata of this instance
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// If instance is enabled to accept request. The default value is true.
        /// </summary>
        public bool InstanceEnabled { get; set; } = true;

        /// <summary>
        /// If instance is ephemeral.The default value is true.
        /// </summary>
        public bool Ephemeral { get; set; } = true;

        /// <summary>
        /// whether your service is a https service.
        /// </summary>
        public bool Secure { get; set; } = false;

        /// <summary>
        /// Load Balance Strategy
        /// </summary>
        public string LBStrategy { get; set; } = LBStrategyName.WeightRandom.ToString();

        public System.Action<NacosSdkOptions> BuildSdkOptions()
        {
            return x =>
            {
                x.AccessKey = this.AccessKey;
                x.ConfigUseRpc = this.ConfigUseRpc;
                x.ContextPath = this.ContextPath;
                x.DefaultTimeOut = this.DefaultTimeOut;
                x.EndPoint = this.EndPoint;
                x.ListenInterval = this.ListenInterval;
                x.Namespace = this.Namespace;
                x.NamingLoadCacheAtStart = this.NamingLoadCacheAtStart;
                x.NamingUseRpc = this.NamingUseRpc;
                x.Password = this.Password;
                x.SecretKey = this.SecretKey;
                x.ServerAddresses = this.ServerAddresses;
                x.UserName = this.UserName;
            };
        }
    }
}
