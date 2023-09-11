namespace Nacos.Remote.Requests
{
    public class ConfigReSyncRequest : CommonRequest
    {
        public ConfigReSyncRequest(string dataId, string group, string tenant)
        {
            Tenant = tenant;
            DataId = dataId;
            Group = group;
        }

        /// <summary>
        /// The tenant, corresponding to the namespace field of Nacos
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("tenant")]
        public string Tenant { get; private set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("dataId")]
        public string DataId { get; private set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("group")]
        public string Group { get; private set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Config_ReSync;
    }
}
