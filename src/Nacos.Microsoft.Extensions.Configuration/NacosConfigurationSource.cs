namespace Nacos.Microsoft.Extensions.Configuration
{
    using global::Microsoft.Extensions.Configuration;
    using System.Collections.Generic;

    public class NacosConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Nacos Server Addresses
        /// </summary>
        public List<string> ServerAddresses { get; set; }

        /// <summary>
        /// Determines if the Nacos Server is optional
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        public string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// EndPoint
        /// </summary>
        public string EndPoint { get; set; }

        public string ContextPath { get; set; } = "nacos";

        public string ClusterName { get; set; } = "serverlist";

        /// <summary>
        /// default timeout, unit is Milliseconds.
        /// </summary>
        public int DefaultTimeOut { get; set; } = 15000;

        /// <summary>
        /// accessKey
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// secretKey
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The configuration parser, default is json
        /// </summary>
        public INacosConfigurationParser NacosConfigurationParser { get; set; }

        /// <summary>
        /// Build the provider
        /// </summary>
        /// <param name="builder">builder</param>
        /// <returns>IConfigurationProvider</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new NacosConfigurationProvider(this);
        }
    }
}
