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

    public class ConfigQueryRequest : Nacos.Remote.CommonRequest
    {
        public ConfigQueryRequest(string dataId, string group, string tenant)
        {
            this.Tenant = tenant;
            this.DataId = dataId;
            this.Group = group;
        }

        /// <summary>
        /// The tenant, corresponding to the namespace field of Nacos
        /// </summary>
        [Newtonsoft.Json.JsonProperty("tenant")]
        public string Tenant { get; private set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [Newtonsoft.Json.JsonProperty("dataId")]
        public string DataId { get; private set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [Newtonsoft.Json.JsonProperty("group")]
        public string Group { get; private set; }

        [Newtonsoft.Json.JsonProperty("tag")]
        public string Tag { get; set; }
    }
}
