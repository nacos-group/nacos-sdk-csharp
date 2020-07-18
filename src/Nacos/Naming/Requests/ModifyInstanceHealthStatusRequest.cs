namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;

    public class ModifyInstanceHealthStatusRequest : BaseRequest
    {
        /// <summary>
        /// ip of instance
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// port of instance
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// namespace id
        /// </summary>
        public string NamespaceId { get; set; }

        /// <summary>
        /// if healthy
        /// </summary>
        public bool Healthy { get; set; }

        /// <summary>
        /// cluster name
        /// </summary>
        public string ClusterName { get; set; }

        /// <summary>
        /// group name
        /// </summary>
        public string GroupName { get; set; }

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
                { "healthy", Healthy.ToString() },
            };

            if (!string.IsNullOrWhiteSpace(NamespaceId))
                dict.Add("namespaceId", NamespaceId);

            if (!string.IsNullOrWhiteSpace(GroupName))
                dict.Add("groupName", GroupName);

            if (!string.IsNullOrWhiteSpace(ClusterName))
                dict.Add("clusterName", ClusterName);

            return dict;
        }
    }
}
