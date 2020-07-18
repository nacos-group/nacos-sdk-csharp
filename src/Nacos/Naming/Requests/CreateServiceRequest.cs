namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;
    using System.Text;

    public class CreateServiceRequest : BaseRequest
    {
        /// <summary>
        /// service name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// namespace id
        /// </summary>
        public string NamespaceId { get; set; }

        /// <summary>
        /// Protect threshold, set value from 0 to 1, default 0
        /// </summary>
        public float? ProtectThreshold { get; set; }

        /// <summary>
        /// visit strategy, a JSON string
        /// </summary>
        public string Selector { get; set; }

        /// <summary>
        /// metadata of service
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// group name
        /// </summary>
        public string GroupName { get; set; }

        public override void CheckParam()
        {
            ParamUtil.CheckServiceName(ServiceName);
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "serviceName", ServiceName },
            };

            if (!string.IsNullOrWhiteSpace(NamespaceId))
                dict.Add("namespaceId", NamespaceId);

            if (!string.IsNullOrWhiteSpace(Metadata))
                dict.Add("metadata", Metadata);

            if (!string.IsNullOrWhiteSpace(GroupName))
                dict.Add("groupName", GroupName);

            if (!string.IsNullOrWhiteSpace(Selector))
                dict.Add("selector", Selector);

            if (ProtectThreshold.HasValue)
                dict.Add("protectThreshold", ProtectThreshold.ToString());

            return dict;
        }
    }
}
