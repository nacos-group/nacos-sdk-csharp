namespace Nacos.Naming.Dtos
{
    using Nacos.Common;
    using Nacos.Naming.Core;
    using Nacos.Utils;
    using System.Collections.Generic;
    using System.Linq;

    public class Instance
    {
        /// <summary>
        /// unique id of this instance.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("instanceId")]
        public string InstanceId { get; set; }

        /// <summary>
        /// instance ip.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("ip")]
        public string Ip { get; set; }

        /// <summary>
        /// instance port.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        /// instance weight.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("weight")]
        public double Weight { get; set; } = 1.0D;

        /// <summary>
        /// instance health status.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("healthy")]
        public bool Healthy { get; set; } = true;

        /// <summary>
        /// If instance is enabled to accept request.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// If instance is ephemeral.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("ephemeral")]
        public bool Ephemeral { get; set; } = true;

        /// <summary>
        /// cluster information of instance.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("clusterName")]
        public string ClusterName { get; set; }

        /// <summary>
        ///  Service information of instance.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("serviceName")]
        public string ServiceName { get; set; }

        /// <summary>
        /// user extended attributes.
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public void AddMetadata(string key, string value)
        {
            if (Metadata == null) Metadata = new Dictionary<string, string>();

            Metadata[key] = value;
        }

        public string ToInetAddr() => $"{Ip}:{Port}";

        public long GetInstanceHeartBeatInterval() => GetMetaDataByKeyWithDefault(PreservedMetadataKeys.HEART_BEAT_INTERVAL, Constants.DEFAULT_HEART_BEAT_INTERVAL);

        public long GetInstanceHeartBeatTimeOut() => GetMetaDataByKeyWithDefault(PreservedMetadataKeys.HEART_BEAT_TIMEOUT, Constants.DEFAULT_HEART_BEAT_TIMEOUT);

        public long GetIpDeleteTimeout() => GetMetaDataByKeyWithDefault(PreservedMetadataKeys.IP_DELETE_TIMEOUT, Constants.DEFAULT_IP_DELETE_TIMEOUT);

        public string GetInstanceIdGenerator() => GetMetaDataByKeyWithDefault(PreservedMetadataKeys.INSTANCE_ID_GENERATOR, Constants.DEFAULT_INSTANCE_ID_GENERATOR);

        private long GetMetaDataByKeyWithDefault(string key, long defaultValue)
        {
            var exist = GetMetaDataByKey(key, out string value);
            if (exist && long.TryParse(value, out _)) return long.Parse(value);

            return defaultValue;
        }

        private string GetMetaDataByKeyWithDefault(string key, string defaultValue)
        {
            var exist = GetMetaDataByKey(key, out string value);
            if (exist) return value;

            return defaultValue;
        }

        private bool GetMetaDataByKey(string key, out string vaule)
        {
            vaule = string.Empty;
            if (Metadata == null || !Metadata.Any()) return false;

            if (Metadata.TryGetValue(key, out var val)
                && !string.IsNullOrWhiteSpace(val))
            {
                vaule = val;
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            return $"Instance{{instanceId='{InstanceId}', ip='{Ip}', port={Port}, weight={Weight}, healthy={Healthy}, enabled={Enabled}, ephemeral={Ephemeral}, clusterName='{ClusterName}', serviceName='{ServiceName}', metadata={Metadata.ToJsonString()}}}";
        }

        public override bool Equals(object obj)
        {
            if (obj is not Instance) return false;

            var host = (Instance)obj;

            return ToString().Equals(host.ToString());
        }

        public override int GetHashCode() => ToString().GetHashCode();
    }
}
