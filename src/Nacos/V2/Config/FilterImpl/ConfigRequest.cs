namespace Nacos.V2.Config.FilterImpl
{
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Utils;

    public class ConfigRequest : IConfigRequest
    {
        private System.Collections.Generic.Dictionary<string, object> param = new System.Collections.Generic.Dictionary<string, object>();

        private IConfigContext configContext = new ConfigContext();

        public string GetTenant() => param.SafeGetValue(ConfigConstants.TENANT, null);

        public void SetTenant(string tenant) => param[ConfigConstants.TENANT] = tenant;

        public string GetDataId() => param.SafeGetValue(ConfigConstants.DATA_ID, null);

        public void SetDataId(string dataId) => param[ConfigConstants.DATA_ID] = dataId;

        public string GetGroup() => param.SafeGetValue(ConfigConstants.GROUP, null);

        public void SetGroup(string group) => param[ConfigConstants.GROUP] = group;

        public string GetContent() => param.SafeGetValue(ConfigConstants.CONTENT, null);

        public void SetContent(string content) => param[ConfigConstants.CONTENT] = content;

        public string GetConfigRequestType() => param.SafeGetValue("type", null);

        public void SetType(string type) => param["type"] = type;

        public object GetParameter(string key) => param.TryGetValue(key, out var obj) ? obj : null;

        public IConfigContext GetConfigContext() => configContext;
    }
}
