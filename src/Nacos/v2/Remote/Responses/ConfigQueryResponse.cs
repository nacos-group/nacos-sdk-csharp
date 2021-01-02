namespace Nacos.V2.Remote.Responses
{
    using System.Collections.Generic;

    public class ConfigQueryResponse : CommonResponse
    {
        public static readonly int CONFIG_NOT_FOUND = 300;

        public static readonly int CONFIG_QUERY_CONFLICT = 400;

        [Newtonsoft.Json.JsonProperty("content")]
        public string Content { get; set; }

        [Newtonsoft.Json.JsonProperty("contentType")]
        public string ContentType { get; set; }

        [Newtonsoft.Json.JsonProperty("labels")]
        public Dictionary<string, string> Labels { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Config_Query;
    }
}
