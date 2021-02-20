namespace Nacos.V2.Naming.Beat
{
    using Nacos.V2.Utils;
    using System.Collections.Generic;

    public class BeatInfo
    {
        [Newtonsoft.Json.JsonProperty("port")]
        public int Port { get; set; }

        [Newtonsoft.Json.JsonProperty("ip")]
        public string Ip { get; set; }

        [Newtonsoft.Json.JsonProperty("weight")]
        public double? Weight { get; set; }

        [Newtonsoft.Json.JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [Newtonsoft.Json.JsonProperty("cluster")]
        public string Cluster { get; set; }

        [Newtonsoft.Json.JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        [Newtonsoft.Json.JsonProperty("scheduled")]
        public bool Scheduled { get; set; }

        [Newtonsoft.Json.JsonProperty("period")]
        public long Period { get; set; }

        [Newtonsoft.Json.JsonProperty("stopped")]
        public bool Stopped { get; set; }

        public override string ToString()
        {
            return "BeatInfo{" + "port=" + Port + ", ip='" + Ip + '\'' + ", weight=" + Weight + ", serviceName='" + ServiceName + '\'' + ", cluster='" + Cluster + '\'' + ", metadata=" + Metadata.ToJsonString() + ", scheduled=" + Scheduled + ", period=" + Period + ", stopped=" + Stopped + '}';
        }
    }
}
