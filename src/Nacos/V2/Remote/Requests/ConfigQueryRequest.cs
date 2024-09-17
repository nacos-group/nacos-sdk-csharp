namespace Nacos.V2.Remote.Requests
{
    public class ConfigQueryRequest : CommonRequest
    {
        public ConfigQueryRequest(string dataId, string group, string tenant)
        {
            this.Tenant = tenant;
            this.DataId = dataId;
            this.Group = group;
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

        [System.Text.Json.Serialization.JsonPropertyName("tag")]
        public string Tag { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Config_Get;
    }
}
