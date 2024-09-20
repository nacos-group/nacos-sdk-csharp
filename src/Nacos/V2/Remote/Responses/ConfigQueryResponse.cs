namespace Nacos.V2.Remote.Responses
{
    public class ConfigQueryResponse : CommonResponse
    {
        public static readonly int CONFIG_NOT_FOUND = 300;

        public static readonly int CONFIG_QUERY_CONFLICT = 400;

        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string Content { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("md5")]
        public string Md5 { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("isBeta")]
        public bool IsBeta { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tag")]
        public string Tag { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("lastModified")]
        public long LastModified { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("encryptedDataKey")]
        public string EncryptedDataKey { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Config_Query;
    }
}
