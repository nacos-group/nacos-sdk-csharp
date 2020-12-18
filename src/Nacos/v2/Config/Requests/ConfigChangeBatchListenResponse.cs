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

    public class ConfigChangeBatchListenResponse : Nacos.Remote.CommonRequest
    {
        [Newtonsoft.Json.JsonProperty("changedConfigs")]
        public List<ConfigContext> ChangedConfigs = new List<ConfigContext>();
    }
}
