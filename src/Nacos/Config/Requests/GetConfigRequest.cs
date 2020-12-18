namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;

    public class GetConfigRequest : BaseRequest
    {
        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        public string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        public string Group { get; set; }

        public override void CheckParam()
        {
            ParamUtil.CheckKeyParam(DataId, Group);
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "dataId", DataId },
                { "group", Group }
            };

            if (!string.IsNullOrWhiteSpace(Tenant))
            {
                dict.Add("tenant", Tenant);
            }

            return dict;
        }
    }
}
