namespace Nacos.OpenApi
{
    public class NacosMetrics
    {
        [Newtonsoft.Json.JsonProperty("status")]
        public string Status { get; set; }

        [Newtonsoft.Json.JsonProperty("serviceCount")]
        public int ServiceCount { get; set; }

        [Newtonsoft.Json.JsonProperty("instanceCount")]
        public int InstanceCount { get; set; }

        [Newtonsoft.Json.JsonProperty("subscribeCount")]
        public int SubscribeCount { get; set; }

        [Newtonsoft.Json.JsonProperty("raftNotifyTaskCount")]
        public int RaftNotifyTaskCount { get; set; }

        [Newtonsoft.Json.JsonProperty("responsibleServiceCount")]
        public int ResponsibleServiceCount { get; set; }

        [Newtonsoft.Json.JsonProperty("responsibleInstanceCount")]
        public int ResponsibleInstanceCount { get; set; }

        [Newtonsoft.Json.JsonProperty("clientCount")]
        public int ClientCount { get; set; }

        [Newtonsoft.Json.JsonProperty("connectionBasedClientCount")]
        public int ConnectionBasedClientCount { get; set; }

        [Newtonsoft.Json.JsonProperty("ephemeralIpPortClientCount")]
        public int EphemeralIpPortClientCount { get; set; }

        [Newtonsoft.Json.JsonProperty("persistentIpPortClientCount")]
        public int PersistentIpPortClientCount { get; set; }

        [Newtonsoft.Json.JsonProperty("responsibleClientCount")]
        public int ResponsibleClientCount { get; set; }

        [Newtonsoft.Json.JsonProperty("cpu")]
        public float Cpu { get; set; }

        [Newtonsoft.Json.JsonProperty("load")]
        public float Load { get; set; }

        [Newtonsoft.Json.JsonProperty("mem")]
        public float Mem { get; set; }
    }
}
