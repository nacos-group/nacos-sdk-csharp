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

    public class ConfigContext
    {
        [Newtonsoft.Json.JsonProperty("group")]
        public string Group { get; set; }

        [Newtonsoft.Json.JsonProperty("dataId")]
        public string DataId { get; set; }

        [Newtonsoft.Json.JsonProperty("tenant")]
        public string Tenant { get; set; }
    }
}
