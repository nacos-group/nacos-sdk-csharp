namespace Nacos.V2.Remote.Responses
{
    public class ConfigQueryResponse : CommonResponse
    {
        public static readonly int CONFIG_NOT_FOUND = 300;

        public static readonly int CONFIG_QUERY_CONFLICT = 400;

        [Newtonsoft.Json.JsonProperty("content")]
        public string Content { get; set; }

        [Newtonsoft.Json.JsonProperty("contentType")]
        public string ContentType { get; set; }

        [Newtonsoft.Json.JsonProperty("md5")]
        public string Md5 { get; set; }

        [Newtonsoft.Json.JsonProperty("isBeta")]
        public bool IsBeta { get; set; }

        [Newtonsoft.Json.JsonProperty("tag")]
        public string Tag { get; set; }

        [Newtonsoft.Json.JsonProperty("lastModified")]
        public long LastModified { get; set; }

        [Newtonsoft.Json.JsonProperty("encryptedDataKey")]
        public string EncryptedDataKey { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Config_Query;
    }
}
