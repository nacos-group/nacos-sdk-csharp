namespace Nacos.V2.Common
{
    public class Constants
    {
        public static string CLIENT_VERSION = "Nacos-CSharp-Client:v1.1.0";

        public static int DATA_IN_BODY_VERSION = 204;

        public static string DEFAULT_GROUP = "DEFAULT_GROUP";

        public static string APPNAME = "AppName";

        public static string UNKNOWN_APP = "UnknownApp";

        public static string DEFAULT_DOMAINNAME = "commonconfig.config-host.taobao.com";

        public static string DAILY_DOMAINNAME = "commonconfig.taobao.net";

        public static string NULL = "";

        public static string DATAID = "dataId";

        public static string GROUP = "group";

        public static string LAST_MODIFIED = "Last-Modified";

        public static string ACCEPT_ENCODING = "Accept-Encoding";

        public static string CONTENT_ENCODING = "Content-Encoding";

        public static string PROBE_MODIFY_REQUEST = "Listening-Configs";

        public static string PROBE_MODIFY_RESPONSE = "Probe-Modify-Response";

        public static string PROBE_MODIFY_RESPONSE_NEW = "Probe-Modify-Response-New";

        public static string USE_ZIP = "true";

        public static string CONTENT_MD5 = "Content-MD5";

        public static string CONFIG_VERSION = "Config-Version";

        public static string CONFIG_TYPE = "Config-Type";

        public static string IF_MODIFIED_SINCE = "If-Modified-Since";

        public static string SPACING_INTERVAL = "client-spacing-interval";

        public static string BASE_PATH = "/v1/cs";

        public static string CONFIG_CONTROLLER_PATH = BASE_PATH + "/configs";

        public static string TOKEN = "token";

        public static string ACCESS_TOKEN = "accessToken";

        public static string TOKEN_TTL = "tokenTtl";

        public static string GLOBAL_ADMIN = "globalAdmin";

        public static string TOKEN_REFRESH_WINDOW = "tokenRefreshWindow";

        /// <summary>
        /// second
        /// </summary>
        public static int ASYNC_UPDATE_ADDRESS_INTERVAL = 300;

        /// <summary>
        /// second
        /// </summary>
        public static int POLLING_INTERVAL_TIME = 15;

        /// <summary>
        /// millisecond
        /// </summary>
        public static int ONCE_TIMEOUT = 2000;

        /// <summary>
        /// millisecond.
        /// </summary>
        public static int SO_TIMEOUT = 60000;

        /// <summary>
        /// millisecond.
        /// </summary>
        public static int CONFIG_LONG_POLL_TIMEOUT = 30000;

        /// <summary>
        /// millisecond.
        /// </summary>
        public static int MIN_CONFIG_LONG_POLL_TIMEOUT = 10000;

        /// <summary>
        /// millisecond
        /// </summary>
        public static int CONFIG_RETRY_TIME = 2000;

        /// <summary>
        /// Maximum number of retries.
        /// </summary>
        public static int MAX_RETRY = 3;

        /// <summary>
        /// millisecond
        /// </summary>
        public static int RECV_WAIT_TIMEOUT = ONCE_TIMEOUT * 5;

        public static string ENCODE = "UTF-8";

        public static string MAP_FILE = "map-file.js";

        public static int FLOW_CONTROL_THRESHOLD = 20;

        public static int FLOW_CONTROL_SLOT = 10;

        public static int FLOW_CONTROL_INTERVAL = 1000;

        public static float DEFAULT_PROTECT_THRESHOLD = 0.0F;

        public static string LINE_SEPARATOR = char.ToString((char)1);

        public static string WORD_SEPARATOR = char.ToString((char)2);

        public static string LONGPOLLING_LINE_SEPARATOR = "\r\n";

        public static string CLIENT_APPNAME_HEADER = "Client-AppName";

        public static string CLIENT_REQUEST_TS_HEADER = "Client-RequestTS";

        public static string CLIENT_REQUEST_TOKEN_HEADER = "Client-RequestToken";

        public static int ATOMIC_MAX_SIZE = 1000;

        public static string NAMING_INSTANCE_ID_SPLITTER = "#";

        public static int NAMING_INSTANCE_ID_SEG_COUNT = 4;

        public static string NAMING_HTTP_HEADER_SPILIER = "\\|";

        public static string DEFAULT_CLUSTER_NAME = "DEFAULT";

        public static long DEFAULT_HEART_BEAT_TIMEOUT = 15000;

        public static long DEFAULT_IP_DELETE_TIMEOUT = 30000;

        public static long DEFAULT_HEART_BEAT_INTERVAL = 5000;

        public static string DEFAULT_NAMESPACE_ID = "public";

        public static bool DEFAULT_USE_CLOUD_NAMESPACE_PARSING = true;

        public static int WRITE_REDIRECT_CODE = 307;

        public static string SERVICE_INFO_SPLITER = "@@";

        public static string NULL_string = "null";

        public static string NUMBER_PATTERN = "^\\d+$";

        public static string ANY_PATTERN = ".*";

        public static string DEFAULT_INSTANCE_ID_GENERATOR = "simple";

        public static string SNOWFLAKE_INSTANCE_ID_GENERATOR = "snowflake";

        public static string HTTP_PREFIX = "http";

        public static string ALL_PATTERN = "*";

        public static string COLON = ":";

        public static string TENANT = "tenant";

        public static string VIPSERVER_TAG = "Vipserver-Tag";

        public static string AMORY_TAG = "Amory-Tag";

        public static string LOCATION_TAG = "Location-Tag";
    }
}
