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
        /// </summary>
        public const string ClientVersion = "Nacos-CSharp-Client-v0.2.7";

        /// <summary>
        /// nacos request module
        /// </summary>
        public const string RequestModule = "Naming";

        /// <summary>
        /// default group
        /// </summary>
        public const string DefaultGroup = "DEFAULT_GROUP";

        /// <summary>
        /// default long pulling timeout
        /// </summary>
        public const int LongPullingTimeout = 30;

        /// <summary>
        /// invalid param
        /// </summary>
        public const int CLIENT_INVALID_PARAM = -400;

        /// <summary>
        /// over client threshold
        /// </summary>
        public const int CLIENT_OVER_THRESHOLD = -503;

        /// <summary>
        /// invalid param
        /// </summary>
        public const int INVALID_PARAM = 400;

        /// <summary>
        /// no right
        /// </summary>
        public const int NO_RIGHT = 403;

        /// <summary>
        ///  not found
        /// </summary>
        public const int NOT_FOUND = 404;

        /// <summary>
        /// conflict
        /// </summary>
        public const int CONFLICT = 409;

        /// <summary>
        /// conflict
        /// </summary>
        public const int SERVER_ERROR = 500;

        /// <summary>
        /// bad gateway
        /// </summary>
        public const int BAD_GATEWAY = 502;

        /// <summary>
        /// over threshold
        /// </summary>
        public const int OVER_THRESHOLD = 503;

        public static string ENV_LIST_KEY = "envList";

        public static string ALL_IPS = "000--00-ALL_IPS--00--000";

        public static string FAILOVER_SWITCH = "00-00---000-VIPSRV_FAILOVER_SWITCH-000---00-00";

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
