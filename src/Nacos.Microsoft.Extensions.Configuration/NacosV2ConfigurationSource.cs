namespace Nacos.Microsoft.Extensions.Configuration
{
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using Nacos.Config;
    using System;
    using System.Collections.Generic;

    public class NacosV2ConfigurationSource : Nacos.V2.NacosSdkOptions, IConfigurationSource
    {
        /// <summary>
        /// The configuration listeners
        /// </summary>
        public List<ConfigListener> Listeners { get; set; }

        /// <summary>
        /// Determines if the Nacos Server is optional
        /// </summary>
        [System.Obsolete("please use Listeners to configure")]
        public bool Optional { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        [System.Obsolete("please use Listeners to configure")]
        public string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        [System.Obsolete("please use Listeners to configure")]
        public string Group { get; set; }

        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// The configuration parser, default is json
        /// </summary>
        public INacosConfigurationParser NacosConfigurationParser { get; set; }

        /// <summary>
        /// The logging builder, default will use AddConsole
        /// </summary>
        public Action<ILoggingBuilder> LoggingBuilder { get; set; }

        /// <summary>
        /// Build the provider
        /// </summary>
        /// <param name="builder">builder</param>
        /// <returns>IConfigurationProvider</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new NacosV2ConfigurationProvider(this);
        }
    }
}
