namespace Nacos.Config.Requests
{
    public class ConfigContext
    {
        [Newtonsoft.Json.JsonProperty("group")]
        public string Group { get; set; }

        [Newtonsoft.Json.JsonProperty("dataId")]
        public string DataId { get; set; }

        [Newtonsoft.Json.JsonProperty("tenant")]
        public string Tenant { get; set; }
    }
}
