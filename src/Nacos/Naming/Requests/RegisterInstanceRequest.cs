namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;
    using System.Text;

    public class RegisterInstanceRequest : BaseRequest
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
        /// service name
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
        /// enabled or not
        /// </summary>
        public bool? Enable { get; set; }

        /// <summary>
        /// healthy or not
        /// </summary>
        public bool? Healthy { get; set; }

        /// <summary>
        /// extended information
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// cluster name
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

        public override void CheckParam()
        {
            ParamUtil.CheckInstanceInfo(Ip, Port, ServiceName);
        }

        public override Dictionary<string, string> ToDict()
        {
            string metadata = DictToString(Metadata, "");
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

            if (Metadata.Count != 0)
                dict.Add("metadata", metadata);

            if (Weight.HasValue)
                dict.Add("weight", Weight.Value.ToString());

            if (!string.IsNullOrWhiteSpace(ClusterName))
                dict.Add("clusterName", ClusterName);

            if (Ephemeral.HasValue)
                dict.Add("ephemeral", Ephemeral.Value.ToString());

            if (Healthy.HasValue)
                dict.Add("healthy", Healthy.Value.ToString());

            return dict;
        }

        public string DictToString<T, TV>(IEnumerable<KeyValuePair<T, TV>> items, string format)
        {
            format = string.IsNullOrEmpty(format) ? "{0}='{1}' " : format;

            StringBuilder itemString = new StringBuilder();
            foreach (var item in items)
                itemString.AppendFormat(format, item.Key, item.Value);

            return itemString.ToString();
        }
    }
}
