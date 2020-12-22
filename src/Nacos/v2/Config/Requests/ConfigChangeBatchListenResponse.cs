namespace Nacos.Config.Requests
{
    using System.Collections.Generic;

    public class ConfigChangeBatchListenResponse : Nacos.Remote.CommonResponse
    {
        [Newtonsoft.Json.JsonProperty("changedConfigs")]
        public List<ConfigContext> ChangedConfigs = new List<ConfigContext>();
    }
}
