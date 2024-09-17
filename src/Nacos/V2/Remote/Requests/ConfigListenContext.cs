namespace Nacos.V2.Remote.Requests
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

        [System.Text.Json.Serialization.JsonPropertyName("group")]
        public string Group { get; private set; }

        [System.Text.Json.Serialization.JsonPropertyName("md5")]
        public string Md5 { get; private set; }

        [System.Text.Json.Serialization.JsonPropertyName("dataId")]
        public string DataId { get; private set; }

        [System.Text.Json.Serialization.JsonPropertyName("tenant")]
        public string Tenant { get; private set; }
    }
}
