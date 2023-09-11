namespace Nacos.Remote.Responses
{
    using Nacos.Remote;
    using Nacos.Remote.Requests;
    using System.Collections.Generic;

    public class ConfigChangeBatchListenResponse : CommonResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("changedConfigs")]
        public List<ConfigContext> ChangedConfigs = new List<ConfigContext>();

        public override string GetRemoteType() => RemoteRequestType.Resp_Config_BatchListen;
    }
}
