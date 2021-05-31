namespace Nacos.V2.Config.FilterImpl
{
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Utils;

    public class ConfigResponse : IConfigResponse
    {
        private System.Collections.Generic.Dictionary<string, object> param = new System.Collections.Generic.Dictionary<string, object>();

        private IConfigContext configContext = new ConfigContext();

        public string GetTenant() => param.SafeGetValue(ConfigConstants.TENANT);

        public void SetTenant(string tenant) => param[ConfigConstants.TENANT] = tenant;

        public string GetDataId() => param.SafeGetValue(ConfigConstants.DATA_ID);

        public void SetDataId(string dataId) => param[ConfigConstants.DATA_ID] = dataId;

        public string GetGroup() => param.SafeGetValue(ConfigConstants.GROUP);

        public void SetGroup(string group) => param[ConfigConstants.GROUP] = group;

        public string GetContent() => param.SafeGetValue(ConfigConstants.CONTENT, null);

        public void SetContent(string content) => param[ConfigConstants.CONTENT] = content;

        public string GetConfigType() => param.SafeGetValue(ConfigConstants.CONFIG_TYPE);

        public void SetConfigType(string configType) => param[ConfigConstants.CONFIG_TYPE] = configType;

        public string GetEncryptedDataKey() => param.SafeGetValue(ConfigConstants.ENCRYPTED_DATA_KEY);

        public void SetEncryptedDataKey(string encryptedDataKey) => param[ConfigConstants.ENCRYPTED_DATA_KEY] = encryptedDataKey;

        public object GetParameter(string key) => param.TryGetValue(key, out var obj) ? obj : null;

        public IConfigContext GetConfigContext() => configContext;

        public void PutParameter(string key, object value) => param[key] = value;
    }
}
