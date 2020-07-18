namespace Nacos
{
    using System;
    using System.Collections.Generic;

    public class GetSwitchesResult
    {
        public string Name { get; set; }

        public string Masters { get; set; }

        public object AdWeightMap { get; set; }

        public int DefaultPushCacheMillis { get; set; }

        public int ClientBeatInterval { get; set; }

        public int DefaultCacheMillis { get; set; }

        public double DistroThreshold { get; set; }

        public bool HealthCheckEnabled { get; set; }

        public bool DistroEnabled { get; set; }

        public bool EnableStandalone { get; set; }

        public bool PushEnabled { get; set; }

        public int CheckTimes { get; set; }

        public HttpHealthParams HttpHealthParams { get; set; }

        public TcpHealthParams TcpHealthParams { get; set; }

        public MySqlHealthParams MysqlHealthParams { get; set; }

        public List<string> IncrementalList { get; set; }

        public int ServerStatusSynchronizationPeriodMillis { get; set; }

        public int ServiceStatusSynchronizationPeriodMillis { get; set; }

        public bool DisableAddIP { get; set; }

        public bool SendBeatOnly { get; set; }

        public Dictionary<string, string> LimitedUrlMap { get; set; }

        public int DistroServerExpiredMillis { get; set; }

        public string PushGoVersion { get; set; }

        public string PushJavaVersion { get; set; }

        public string PushPythonVersion { get; set; }

        public string PushCVersion { get; set; }

        public bool EnableAuthentication { get; set; }

        public string OverriddenServerStatus { get; set; }

        public bool DefaultInstanceEphemeral { get; set; }

        public List<string> HealthCheckWhiteList { get; set; }

        public string Checksum { get; set; }
    }
}
