namespace Nacos
{
    public static class RequestPathValue
    {
        public const string CONFIGS = "/nacos/v1/cs/configs";
        public const string CONFIGS_LISTENER = "/nacos/v1/cs/configs/listener";

        public const string INSTANCE = "/nacos/v1/ns/instance";
        public const string INSTANCE_LIST = "/nacos/v1/ns/instance/list";
        public const string INSTANCE_BEAT = "/nacos/v1/ns/instance/beat";
        public const string INSTANCE_HEALTH = "/nacos/v1/ns/health/instance";

        public const string SERVICE = "/nacos/v1/ns/service";
        public const string SERVICE_LIST = "/nacos/v1/ns/service/list";

        public const string SWITCHES = "/nacos/v1/ns/operator/switches";

        public const string SERVERS = "/nacos/v1/ns/operator/servers";

        public const string LEADER = "/nacos/v1/ns/raft/leader";

        public const string METRICS = "/nacos/v1/ns/operator/metrics";
    }
}
