namespace Nacos.Remote.Requests
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

        [Newtonsoft.Json.JsonProperty("tag")]
        public string Tag { get; set; }

        public override string GetGrpcType() => GRpc.GrpcRequestType.Config_Get;
    }
}
