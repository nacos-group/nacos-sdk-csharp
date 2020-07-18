namespace Nacos
{
    using System;
    using System.Collections.Generic;
    using Nacos.Utilities;

    public class AddListenerRequest : BaseRequest
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

        /// <summary>
        /// Callbacks when configuration was changed
        /// </summary>
        public List<Action<string>> Callbacks { get; set; } = new List<Action<string>>();

        /// <summary>
        /// Configuraton value.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }

        /// <summary>
        /// A packet field indicating the MD5 value of the configuration.
        /// </summary>
        public string ContentMD5 => HashUtil.GetMd5(Content);

        /// <summary>
        /// A request to listen for data packets
        /// </summary>
        public string ListeningConfigs => string.IsNullOrWhiteSpace(Tenant)
            ? $"{DataId}{CharacterUtil.TwoEncode}{Group}{CharacterUtil.TwoEncode}{ContentMD5}{CharacterUtil.OneEncode}"
            : $"{DataId}{CharacterUtil.TwoEncode}{Group}{CharacterUtil.TwoEncode}{ContentMD5}{CharacterUtil.TwoEncode}{Tenant}{CharacterUtil.OneEncode}";

        public override void CheckParam()
        {
            ParamUtil.CheckTDG(Tenant, DataId, Group);
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "Listening-Configs", ListeningConfigs },
                { "dataId", DataId },
                { "group", Group },
            };

            if (!string.IsNullOrWhiteSpace(Tenant))
                dict.Add("tenant", Tenant);

            return dict;
        }
    }
}
