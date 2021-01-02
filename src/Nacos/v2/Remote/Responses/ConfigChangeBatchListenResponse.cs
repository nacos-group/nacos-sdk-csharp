namespace Nacos.V2.Remote.Responses
{
    using Nacos.V2.Remote.Requests;
    using System.Collections.Generic;

    public class ConfigChangeBatchListenResponse : Nacos.V2.Remote.CommonResponse
    {
        [Newtonsoft.Json.JsonProperty("changedConfigs")]
        public List<ConfigContext> ChangedConfigs = new List<ConfigContext>();

        public override string GetRemoteType() => RemoteRequestType.Resp_Config_BatchListen;
    }
}
