namespace Nacos.System.Configuration
{
    using global::System;
    using global::System.Configuration;

    public class NacosConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Nacos Server Addresses
        /// </summary>
        [ConfigurationProperty("serverAddresses", IsRequired = true)]
        public string ServerAddresses => this["serverAddresses"].ToString();

        /// <summary>
        /// The configuration listeners
        /// </summary>
        [ConfigurationProperty("listeners", IsRequired = true)]
        public ConfigListenerCollection Listeners => this["listeners"] as ConfigListenerCollection;

        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        [ConfigurationProperty("tenant", IsRequired = true)]
        public string Tenant => this["tenant"]?.ToString();

        /// <summary>
        /// EndPoint
        /// </summary>
        [ConfigurationProperty("endPoint", IsRequired = false)]
        public string EndPoint => this["endPoint"]?.ToString();

        [ConfigurationProperty("contextPath", DefaultValue = "nacos")]
        public string ContextPath => this["contextPath"]?.ToString();

        [ConfigurationProperty("clusterName", DefaultValue = "serverlist")]
        public string ClusterName => this["clusterName"]?.ToString();

        /// <summary>
        /// default timeout, unit is Milliseconds.
        /// </summary>
        [ConfigurationProperty("defaultTimeOut", DefaultValue = 15000)]
        public int DefaultTimeOut => Convert.ToInt32(this["defaultTimeOut"]);

        /// <summary>
        /// accessKey
        /// </summary>
        [ConfigurationProperty("accessKey")]
        public string AccessKey => this["accessKey"]?.ToString();

        /// <summary>
        /// secretKey
        /// </summary>
        [ConfigurationProperty("secretKey")]
        public string SecretKey => this["secretKey"]?.ToString();

        /// <summary>
        /// useGrpc
        /// </summary>
        [ConfigurationProperty("useGrpc", DefaultValue = false)]
        public bool UseGrpc => Convert.ToBoolean(this["useGrpc"]);

        /// <summary>
        /// username
        /// </summary>
        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName => this["userName"]?.ToString();

        /// <summary>
        /// password
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password => this["password"]?.ToString();

        public static NacosConfigurationSection GetConfig(string sectionName) => ConfigurationManager.GetSection(string.IsNullOrWhiteSpace(sectionName) ? "nacos" : sectionName) as NacosConfigurationSection;
    }
}
