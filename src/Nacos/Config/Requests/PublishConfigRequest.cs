namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;

    public class PublishConfigRequest : BaseRequest
    {
        /// <summary>
        /// The tenant, corresponding to the namespace field of Nacos
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

        /// <summary>
        /// Configuration content
        /// </summary>
        [Newtonsoft.Json.JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Configuration type, options value text, json, xml, yaml, html, properties
        /// </summary>
        [Newtonsoft.Json.JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Configuration application
        /// </summary>
        [Newtonsoft.Json.JsonProperty("appName")]
        public string AppName { get; set; }

        /// <summary>
        /// Configuration tags
        /// </summary>
        [Newtonsoft.Json.JsonProperty("tag")]
        public string Tag { get; set; }

        public override void CheckParam()
        {
            ParamUtil.CheckParam(DataId, Group, Content);
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "dataId", DataId },
                { "group", Group },
                { "content", Content }
            };

            if (!string.IsNullOrWhiteSpace(Tenant))
                dict.Add("tenant", Tenant);

            if (!string.IsNullOrWhiteSpace(Type))
                dict.Add("type", Type);

            if (!string.IsNullOrWhiteSpace(AppName))
                dict.Add("appName", AppName);

            if (!string.IsNullOrWhiteSpace(Tag))
                dict.Add("tag", Tag);

            return dict;
        }
    }
}
