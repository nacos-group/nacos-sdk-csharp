namespace Nacos.OpenApi
{
    public class NacosNamespace
    {
        [Newtonsoft.Json.JsonProperty("namespace")]
        public string Namespace { get; set; }

        [Newtonsoft.Json.JsonProperty("namespaceShowName")]
        public string NamespaceShowName { get; set; }

        [Newtonsoft.Json.JsonProperty("namespaceDesc")]
        public string NamespaceDesc { get; set; }

        [Newtonsoft.Json.JsonProperty("quota")]
        public int Quota { get; set; }

        [Newtonsoft.Json.JsonProperty("configCount")]
        public int ConfigCount { get; set; }

        [Newtonsoft.Json.JsonProperty("type")]
        public int Type { get; set; }
    }
}
