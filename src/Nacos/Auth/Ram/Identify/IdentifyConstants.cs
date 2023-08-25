namespace Nacos.Auth.Ram.Identify
{
    public class IdentifyConstants
    {
        public static readonly string ACCESS_KEY = "accessKey";

        public static readonly string SECRET_KEY = "secretKey";

        public static readonly string SECURITY_TOKEN_HEADER = "Spas-SecurityToken";

        public static readonly string TENANT_ID = "tenantId";

        public static readonly string PROPERTIES_FILENAME = "spas.properties";

        public static readonly string CREDENTIAL_PATH = "/home/admin/.spas_key/";

        public static readonly string CREDENTIAL_DEFAULT = "default";

        public static readonly string DOCKER_CREDENTIAL_PATH = "/etc/instanceInfo";

        public static readonly string DOCKER_ACCESS_KEY = "env_spas_accessKey";

        public static readonly string DOCKER_SECRET_KEY = "env_spas_secretKey";

        public static readonly string DOCKER_TENANT_ID = "ebv_spas_tenantId";

        public static readonly string ENV_ACCESS_KEY = "spas_accessKey";

        public static readonly string ENV_SECRET_KEY = "spas_secretKey";

        public static readonly string ENV_TENANT_ID = "tenant.id";

        public static readonly string NO_APP_NAME = "";

        public static readonly string PROJECT_NAME_PROPERTY = "project.name";

        public static readonly string RAM_ROLE_NAME_PROPERTY = "ram.role.name";

        public static readonly string REFRESH_TIME_PROPERTY = "time.to.refresh.in.millisecond";

        public static readonly string SECURITY_PROPERTY = "security.credentials";

        public static readonly string SECURITY_URL_PROPERTY = "security.credentials.url";

        public static readonly string SECURITY_CACHE_PROPERTY = "cache.security.credentials";
    }
}
