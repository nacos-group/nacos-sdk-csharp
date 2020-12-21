namespace Nacos.Config.Requests
{
    using System.Collections.Generic;

    public class ConfigChangeBatchListenResponse : Nacos.Remote.CommonRequest
    {
        [Newtonsoft.Json.JsonProperty("changedConfigs")]
        public List<ConfigContext> ChangedConfigs = new List<ConfigContext>();
    }
}
