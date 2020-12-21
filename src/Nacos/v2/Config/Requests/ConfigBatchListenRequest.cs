namespace Nacos.Config.Requests
{
    using System.Collections.Generic;

    public class ConfigBatchListenRequest
    {
        public List<ConfigListenContext> ConfigListenContexts { get; set; } = new List<ConfigListenContext>();

        public bool Listen { get; set; } = true;

        public void AddConfigListenContext(string tenant, string group, string dataId, string md5)
        {
            var ctx = new ConfigListenContext(tenant, group, dataId, md5);
            this.ConfigListenContexts.Add(ctx);
        }
    }
}
