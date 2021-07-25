namespace Nacos.V2.Naming.Utils
{
    public class UtilAndComs
    {
        public static string WebContext = "/nacos";

        public static string NacosUrlBase = WebContext + "/v1/ns";

        public static string NacosUrlInstance = NacosUrlBase + "/instance";

        public static string NacosUrlService = NacosUrlBase + "/service";

        public static string ENCODING = "UTF-8";

        public static string ENV_LIST_KEY = "envList";

        public static string ALL_IPS = "000--00-ALL_IPS--00--000";

        public static string FAILOVER_SWITCH = "00-00---000-VIPSRV_FAILOVER_SWITCH-000---00-00";

        public static string DEFAULT_NAMESPACE_ID = "public";

        public static int REQUEST_DOMAIN_RETRY_COUNT = 3;

        public static int DEFAULT_CLIENT_BEAT_THREAD_COUNT = 1;

        public static int DEFAULT_POLLING_THREAD_COUNT = 1;

        public static string HTTP = "http://";

        public static string HTTPS = "https://";

        public static string ENV_CONFIGS = "00-00---000-ENV_CONFIGS-000---00-00";

        public static string VIP_CLIENT_FILE = "vipclient.properties";

        public static string ALL_HOSTS = "00-00---000-ALL_HOSTS-000---00-00";
    }
}
