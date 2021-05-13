namespace Nacos.V2.Config.FilterImpl
{
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Utils;

    public class ConfigResponse : IConfigResponse
    {
        private System.Collections.Generic.Dictionary<string, object> param = new System.Collections.Generic.Dictionary<string, object>();

        private IConfigContext configContext = new ConfigContext();

        public string GetTenant() => param.SafeGetValue("tenant");

        public void SetTenant(string tenant) => param["tenant"] = tenant;

        public string GetDataId() => param.SafeGetValue("dataId");

        public void SetDataId(string dataId) => param["dataId"] = dataId;

        public string GetGroup() => param.SafeGetValue("group");

        public void SetGroup(string group) => param["group"] = group;

        public string GetContent() => param.SafeGetValue("content", null);

        public void SetContent(string content) => param["content"] = content;

        public string GetConfigType() => param.SafeGetValue("configType");

        public void SetConfigType(string configType) => param["configType"] = configType;

        public string GetEncryptedDataKey() => param.SafeGetValue("encryptedDataKey");

        public void SetEncryptedDataKey(string encryptedDataKey) => param["encryptedDataKey"] = encryptedDataKey;

        public object GetParameter(string key) => param.TryGetValue(key, out var obj) ? obj : null;

        public IConfigContext GetConfigContext() => configContext;

        public void PutParameter(string key, object value) => param[key] = value;
    }
}
