namespace Nacos
{
    using System.Collections.Generic;
    using System.Text;

    public class ListServicesRequest : BaseRequest
    {
        /// <summary>
        /// current page number
        /// </summary>
        public int PageNo { get; set; }

        /// <summary>
        /// page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// namespace id
        /// </summary>
        public string NamespaceId { get; set; }

        /// <summary>
        /// group name
        /// </summary>
        public string GroupName { get; set; }

        public override void CheckParam()
        {
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "pageNo", PageNo.ToString() },
                { "pageSize", PageSize.ToString() },
            };

            if (!string.IsNullOrWhiteSpace(NamespaceId))
                dict.Add("namespaceId", NamespaceId);

            if (!string.IsNullOrWhiteSpace(GroupName))
                dict.Add("groupName", GroupName);

            return dict;
        }
    }
}
