namespace Nacos.Remote.Responses
{
    using System.Collections.Generic;

    public class ConfigQueryResponse : Nacos.Remote.CommonResponse
    {
        public static readonly int CONFIG_NOT_FOUND = 300;

        public static readonly int CONFIG_QUERY_CONFLICT = 400;

        [Newtonsoft.Json.JsonProperty("content")]
        public string Content { get; set; }

        [Newtonsoft.Json.JsonProperty("contentType")]
        public string ContentType { get; set; }

        [Newtonsoft.Json.JsonProperty("labels")]
        public Dictionary<string, string> Labels { get; set; }
    }
}
