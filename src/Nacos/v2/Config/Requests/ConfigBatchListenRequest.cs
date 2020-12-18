namespace Nacos.Config.Requests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Grpc.Net.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Config.Abst;

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
