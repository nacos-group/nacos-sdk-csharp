namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;

    public class ListenerConfigRequest : BaseRequest
    {
        /// <summary>
        /// A packet field indicating the configuration ID.
        /// </summary>
        public string DataId { get; set; }

        /// <summary>
        /// A packet field indicating the configuration group.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// A packet field indicating the MD5 value of the configuration.
        /// </summary>
        public string ContentMD5 => HashUtil.GetMd5(Content);

        /// <summary>
        /// A packet field indicating tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// The configuration value
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// A request to listen for data packets
        /// </summary>
        public string ListeningConfigs => string.IsNullOrWhiteSpace(Tenant)
            ? $"{DataId}{CharacterUtil.TwoEncode}{Group}{CharacterUtil.TwoEncode}{ContentMD5}{CharacterUtil.OneEncode}"
            : $"{DataId}{CharacterUtil.TwoEncode}{Group}{CharacterUtil.TwoEncode}{ContentMD5}{CharacterUtil.TwoEncode}{Tenant}{CharacterUtil.OneEncode}";

        public override void CheckParam()
        {
        }

        public override Dictionary<string, string> ToDict()
        {
            return new Dictionary<string, string>()
            {
                { "Listening-Configs", ListeningConfigs }
            };
        }
    }
}
