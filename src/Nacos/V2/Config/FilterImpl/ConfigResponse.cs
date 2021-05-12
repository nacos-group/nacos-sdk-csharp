namespace Nacos.V2.Config.FilterImpl
{
    using Nacos.V2.Config.Abst;

    public class ConfigResponse : IConfigResponse
    {
        private System.Collections.Generic.Dictionary<string, object> param = new System.Collections.Generic.Dictionary<string, object>();

        private IConfigContext configContext = new ConfigContext();

        public string GetTenant()
        {
            return (string)param["tenant"];
        }

        public void SetTenant(string tenant)
        {
            param["tenant"] = tenant;
        }

        public string GetDataId()
        {
            return (string)param["dataId"];
        }

        public void SetDataId(string dataId)
        {
            param["dataId"] = dataId;
        }

        public string GetGroup()
        {
            return (string)param["group"];
        }

        public void SetGroup(string group)
        {
            param["group"] = group;
        }

        public string GetContent()
        {
            return (string)param["content"];
        }

        public void SetContent(string content)
        {
            param["content"] = content;
        }

        public string GetConfigType()
        {
            return (string)param["configType"];
        }

        public void SetConfigType(string configType)
        {
            param["configType"] = configType;
        }

        public string GetEncryptedDataKey()
        {
            return (string)param["encryptedDataKey"];
        }

        public void SetEncryptedDataKey(string encryptedDataKey)
        {
            param["encryptedDataKey"] = encryptedDataKey;
        }

        public object GetParameter(string key)
        {
            return param[key];
        }

        public IConfigContext GetConfigContext()
        {
            return configContext;
        }

        public void PutParameter(string key, object value)
        {
            param[key] = value;
        }
    }
}
