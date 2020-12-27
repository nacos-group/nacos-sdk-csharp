namespace Nacos.Config.Requests
{
    using System.Collections.Generic;

    /*
     {"headers":{},"requestId":"7","dataId":"123","group":"g","tenant":"test","beta":false,"contentPush":false,"lastModifiedTs":0,"module":"config"}, {"ClientIp":"172.19.48.1","ClientPort":9848,"Type":"com.alibaba.nacos.api.config.remote.request.ConfigChangeNotifyRequest","ConnectionId":"e60e30bf-539d-412f-a224-50946624e18d","ClientVersion":"Nacos-Java-Client:v2.0.0-ALPHA.1","Labels":{},"Headers":{}}
     */
    public class ConfigChangeNotifyRequest : Nacos.Remote.CommonRequest
    {
        /// <summary>
        /// The tenant, corresponding to the namespace field of Nacos
        /// </summary>
        [Newtonsoft.Json.JsonProperty("tenant")]
        public string Tenant { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [Newtonsoft.Json.JsonProperty("dataId")]
        public string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [Newtonsoft.Json.JsonProperty("group")]
        public string Group { get; set; }

        [Newtonsoft.Json.JsonProperty("beta")]
        public bool Beta { get; set; }

        [Newtonsoft.Json.JsonProperty("contentPush")]
        public bool ContentPush { get; set; }

        [Newtonsoft.Json.JsonProperty("lastModifiedTs")]
        public long LastModifiedTs { get; set; }
    }
}
