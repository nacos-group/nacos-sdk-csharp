namespace Nacos.V2.Naming.Beat
{
    using Nacos.V2.Utils;
    using System.Collections.Generic;

    public class BeatInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("port")]
        public int Port { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ip")]
        public string Ip { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("weight")]
        public double? Weight { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("cluster")]
        public string Cluster { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        [System.Text.Json.Serialization.JsonPropertyName("scheduled")]
        public bool Scheduled { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("period")]
        public long Period { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("stopped")]
        public bool Stopped { get; set; }

        public override string ToString()
        {
            return "BeatInfo{" + "port=" + Port + ", ip='" + Ip + '\'' + ", weight=" + Weight + ", serviceName='" + ServiceName + '\'' + ", cluster='" + Cluster + '\'' + ", metadata=" + Metadata.ToJsonString() + ", scheduled=" + Scheduled + ", period=" + Period + ", stopped=" + Stopped + '}';
        }
    }
}
