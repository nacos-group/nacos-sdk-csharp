namespace Nacos.V2.Config.FilterImpl
{
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Utils;

    public class ConfigRequest : IConfigRequest
    {
        private System.Collections.Generic.Dictionary<string, object> param = new System.Collections.Generic.Dictionary<string, object>();

        private IConfigContext configContext = new ConfigContext();

        public string GetTenant() => param.SafeGetValue("tenant", null);

        public void SetTenant(string tenant) => param["tenant"] = tenant;

        public string GetDataId() => param.SafeGetValue("dataId", null);

        public void SetDataId(string dataId) => param["dataId"] = dataId;

        public string GetGroup() => param.SafeGetValue("group", null);

        public void SetGroup(string group) => param["group"] = group;

        public string GetContent() => param.SafeGetValue("content", null);

        public void SetContent(string content) => param["content"] = content;

        public string GetConfigRequestType() => param.SafeGetValue("type", null);

        public void SetType(string type) => param["type"] = type;

        public object GetParameter(string key) => param.TryGetValue(key, out var obj) ? obj : null;

        public IConfigContext GetConfigContext() => configContext;
    }
}
