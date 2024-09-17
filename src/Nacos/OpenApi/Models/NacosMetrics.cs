namespace Nacos.OpenApi
{
    public class NacosMetrics
    {
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("serviceCount")]
        public int ServiceCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("instanceCount")]
        public int InstanceCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("subscribeCount")]
        public int SubscribeCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("raftNotifyTaskCount")]
        public int RaftNotifyTaskCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("responsibleServiceCount")]
        public int ResponsibleServiceCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("responsibleInstanceCount")]
        public int ResponsibleInstanceCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("clientCount")]
        public int ClientCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("connectionBasedClientCount")]
        public int ConnectionBasedClientCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ephemeralIpPortClientCount")]
        public int EphemeralIpPortClientCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("persistentIpPortClientCount")]
        public int PersistentIpPortClientCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("responsibleClientCount")]
        public int ResponsibleClientCount { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("cpu")]
        public float Cpu { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("load")]
        public float Load { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("mem")]
        public float Mem { get; set; }
    }
}
