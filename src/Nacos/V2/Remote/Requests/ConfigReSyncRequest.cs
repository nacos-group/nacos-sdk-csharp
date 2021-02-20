namespace Nacos.V2.Remote.Requests
{
    public class ConfigReSyncRequest : CommonRequest
    {
        public ConfigReSyncRequest(string dataId, string group, string tenant)
        {
            this.Tenant = tenant;
            this.DataId = dataId;
            this.Group = group;
        }

        /// <summary>
        /// The tenant, corresponding to the namespace field of Nacos
        /// </summary>
        [Newtonsoft.Json.JsonProperty("tenant")]
        public string Tenant { get; private set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [Newtonsoft.Json.JsonProperty("dataId")]
        public string DataId { get; private set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [Newtonsoft.Json.JsonProperty("group")]
        public string Group { get; private set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Config_ReSync;
    }
}
