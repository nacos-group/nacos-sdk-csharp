namespace Nacos.V2.Remote.Requests
{
    using System.Collections.Generic;

    /*
     {"headers":{},"requestId":"7","dataId":"123","group":"g","tenant":"test","beta":false,"contentPush":false,"lastModifiedTs":0,"module":"config"}, {"ClientIp":"172.19.48.1","ClientPort":9848,"Type":"com.alibaba.nacos.api.config.remote.request.ConfigChangeNotifyRequest","ConnectionId":"e60e30bf-539d-412f-a224-50946624e18d","ClientVersion":"Nacos-Java-Client:v2.0.0-ALPHA.1","Labels":{},"Headers":{}}
     */
    public class ConfigChangeNotifyRequest : Nacos.V2.Remote.CommonRequest
    {
        /// <summary>
        /// The tenant, corresponding to the namespace field of Nacos
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("tenant")]
        public string Tenant { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("dataId")]
        public string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("group")]
        public string Group { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("beta")]
        public bool Beta { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("contentPush")]
        public bool ContentPush { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("lastModifiedTs")]
        public long LastModifiedTs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("content")]
        public string Content { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Config_ChangeNotify;
    }
}
