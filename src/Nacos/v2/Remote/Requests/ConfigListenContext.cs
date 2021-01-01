namespace Nacos.Remote.Requests
{
    public class ConfigListenContext
    {
        public ConfigListenContext(string tenant, string group, string dataId, string md5)
        {
            this.Tenant = tenant;
            this.Group = group;
            this.DataId = dataId;
            this.Md5 = md5;
        }

        [Newtonsoft.Json.JsonProperty("group")]
        public string Group { get; private set; }

        [Newtonsoft.Json.JsonProperty("md5")]
        public string Md5 { get; private set; }

        [Newtonsoft.Json.JsonProperty("dataId")]
        public string DataId { get; private set; }

        [Newtonsoft.Json.JsonProperty("tenant")]
        public string Tenant { get; private set; }
    }
}
