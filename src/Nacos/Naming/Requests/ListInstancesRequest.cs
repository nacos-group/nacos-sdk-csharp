namespace Nacos
{
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;

    public class ListInstancesRequest : BaseRequest
    {
        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// ID of namespace
        /// </summary>
        public string NamespaceId { get; set; }

        /// <summary>
        /// Cluster name, splited by comma
        /// </summary>
        public string Clusters { get; set; }

        /// <summary>
        /// group name
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Return healthy instance or not
        /// </summary>
        public bool? HealthyOnly { get; set; }

        public int UdpPort { get; set; }

        public string ClientIp { get; set; }

        public override void CheckParam()
        {
            ParamUtil.CheckServiceName(ServiceName);
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "serviceName", ServiceName },
                { "udpPort", UdpPort.ToString() },
            };

            if (!string.IsNullOrWhiteSpace(NamespaceId))
                dict.Add("namespaceId", NamespaceId);

            if (!string.IsNullOrWhiteSpace(Clusters))
                dict.Add("clusters", Clusters);

            if (!string.IsNullOrWhiteSpace(GroupName))
                dict.Add("groupName", GroupName);

            if (HealthyOnly.HasValue)
                dict.Add("healthyOnly", HealthyOnly.Value.ToString());

            if (!string.IsNullOrWhiteSpace(ClientIp))
                dict.Add("clientIP", ClientIp);

            return dict;
        }
    }
}
