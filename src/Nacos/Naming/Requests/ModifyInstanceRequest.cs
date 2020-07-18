namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;

    public class ModifyInstanceRequest : BaseRequest
    {
        /// <summary>
        /// IP of instance
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Port of instance
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// ID of namespace
        /// </summary>
        public string NamespaceId { get; set; }

        /// <summary>
        /// Weight
        /// </summary>
        public double? Weight { get; set; }

        /// <summary>
        /// Extended information, a JSON string
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// Cluster name
        /// </summary>
        public string ClusterName { get; set; }

        /// <summary>
        /// group name
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// if instance is ephemeral
        /// </summary>
        public bool? Ephemeral { get; set; }

        /// <summary>
        /// If enabled
        /// </summary>
        /// <value>The enabled.</value>
        public bool? Enabled { get; set; }

        public override void CheckParam()
        {
            ParamUtil.CheckInstanceInfo(Ip, Port, ServiceName);
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "serviceName", ServiceName },
                { "ip", Ip },
                { "port", Port.ToString() },
            };

            if (!string.IsNullOrWhiteSpace(NamespaceId))
                dict.Add("namespaceId", NamespaceId);

            if (!string.IsNullOrWhiteSpace(GroupName))
                dict.Add("groupName", GroupName);

            if (!string.IsNullOrWhiteSpace(ClusterName))
                dict.Add("clusterName", ClusterName);

            if (!string.IsNullOrWhiteSpace(Metadata))
                dict.Add("metadata", Metadata);

            if (Ephemeral.HasValue)
                dict.Add("ephemeral", Ephemeral.ToString());

            if (Enabled.HasValue)
                dict.Add("enabled", Enabled.Value.ToString());

            if (Weight.HasValue)
                dict.Add("weight", Weight.Value.ToString());

            return dict;
        }
    }
}
