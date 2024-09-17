namespace Nacos.OpenApi
{
    public class NacosNamespace
    {
        [System.Text.Json.Serialization.JsonPropertyName("namespace")]
        public string Namespace { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("namespaceShowName")]
        public string NamespaceShowName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("namespaceDesc")]
        public string NamespaceDesc { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("quota")]
        public int Quota { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("configCount")]
        public int ConfigCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public int Type { get; set; }
    }
}
