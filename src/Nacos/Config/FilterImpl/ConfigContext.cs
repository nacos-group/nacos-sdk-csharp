namespace Nacos.Config.FilterImpl
{
    using Nacos.Config.Abst;

    public class ConfigContext : IConfigContext
    {
        private System.Collections.Generic.Dictionary<string, object> param = new System.Collections.Generic.Dictionary<string, object>();

        public object GetParameter(string key) => param.TryGetValue(key, out var val) ? val : null;

        public void SetParameter(string key, object value) => param[key] = value;
    }
}
