namespace Nacos.Remote.Requests
{
    public class ConfigContext
    {
        [System.Text.Json.Serialization.JsonPropertyName("group")]
        public string Group { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("dataId")]
        public string DataId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tenant")]
        public string Tenant { get; set; }
    }
}
