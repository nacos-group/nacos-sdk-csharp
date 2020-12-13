namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;

    public class GetConfigRequest : BaseRequest
    {
        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        [Newtonsoft.Json.JsonProperty("tenant")]
        public string Tenant { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [Newtonsoft.Json.JsonProperty("dataId")]
        public string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [Newtonsoft.Json.JsonProperty("group")]
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
