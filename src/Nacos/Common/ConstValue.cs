namespace Nacos
{
    public static class ConstValue
    {
        /// <summary>
        /// default httpclient name
        /// </summary>
        public const string ClientName = "NacosClient";

        /// <summary>
        /// nacos csharp client version
        /// public const string ClientVersion = "Nacos-Java-Client:v1.3.0";
        /// </summary>
        public const string ClientVersion = "Nacos-CSharp-Client:v1.0.0";

        /// <summary>
        /// nacos request module
        /// </summary>
        public const string RequestModule = "Naming";

        /// <summary>
        /// default group
        /// </summary>
        public const string DefaultGroup = "DEFAULT_GROUP";

        /// <summary>
        /// ServiceInfoSplitter
        /// </summary>
        public const string ServiceInfoSplitter = "@@";

        /// <summary>
        /// BeatInfoSplitter
        /// </summary>
        public const string BeatInfoSplitter = "#";

        /// <summary>
        /// default long pulling timeout
        /// </summary>
        public const int LongPullingTimeout = 30;

        public static string FAILOVER_SWITCH = "00-00---000-VIPSRV_FAILOVER_SWITCH-000---00-00";
        public static string ALL_IPS = "000--00-ALL_IPS--00--000";
        public static string ENV_CONFIGS = "00-00---000-ENV_CONFIGS-000---00-00";
        public static string VIPCLIENT_CONFIG = "vipclient.properties";
        public static string ALL_HOSTS = "00-00---000-ALL_HOSTS-000---00-00";
        public static string ENV_LIST_KEY = "envList";

        public static string DEFAULT_NAMESPACE_ID = "public";

        public static int REQUEST_DOMAIN_RETRY_COUNT = 3;

        public static string SERVER_ADDR_IP_SPLITER = ":";

        public static string HTTP = "http://";

        public static string HTTPS = "https://";

        public static string HTTP_PREFIX = "http";

        public static string ACCESS_TOKEN = "accessToken";

        public static string TOKEN_TTL = "tokenTtl";
    }
}
