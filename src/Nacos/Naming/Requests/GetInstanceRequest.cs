namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;
    using System.Text;

    public class GetInstanceRequest : BaseRequest
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
        /// Return healthy instance or not
        /// </summary>
        public bool? HealthyOnly { get; set; }

        /// <summary>
        /// Cluster name
        /// </summary>
        public string Cluster { get; set; }

        /// <summary>
        /// group name
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// if instance is ephemeral
        /// </summary>
        public bool? Ephemeral { get; set; }

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

            if (!string.IsNullOrWhiteSpace(Cluster))
                dict.Add("cluster", Cluster);

            if (!string.IsNullOrWhiteSpace(GroupName))
                dict.Add("groupName", GroupName);

            if (HealthyOnly.HasValue)
                dict.Add("healthyOnly", HealthyOnly.Value.ToString());

            if (Ephemeral.HasValue)
                dict.Add("ephemeral", Ephemeral.Value.ToString());

            return dict;
        }
    }
}
