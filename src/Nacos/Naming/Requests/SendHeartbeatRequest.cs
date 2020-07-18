namespace Nacos
{
    using Nacos.Utilities;
    using System.Collections.Generic;

    public class SendHeartbeatRequest : BaseRequest
    {
        /// <summary>
        /// Service Name
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// beat content
        /// </summary>
        public string Beat => BeatInfo.ToJsonString();

        /// <summary>
        /// beat info
        /// </summary>
        public BeatInfo BeatInfo { get; set; }

        /// <summary>
        /// group name
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// if instance is ephemeral
        /// </summary>
        public bool? Ephemeral { get; set; }

        /// <summary>
        /// namespace id
        /// </summary>
        public string NameSpaceId { get; set; }

        public override void CheckParam()
        {
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "serviceName", ServiceName },
                { "beat", Beat },
            };

            if (!string.IsNullOrWhiteSpace(NameSpaceId))
                dict.Add("namespaceId", NameSpaceId);

            if (Ephemeral.HasValue)
                dict.Add("ephemeral", Ephemeral.Value.ToString());

            if (!string.IsNullOrWhiteSpace(GroupName))
                dict.Add("groupName", GroupName);

            return dict;
        }
    }
}
