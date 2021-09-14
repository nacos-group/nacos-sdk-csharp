namespace Nacos.Microsoft.Extensions.Configuration
{
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.Logging;
    using Nacos.V2;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Generic;

    public class NacosV2ConfigurationSource : Nacos.V2.NacosSdkOptions, IConfigurationSource
    {
        /// <summary>
        /// The configuration listeners
        /// </summary>
        public List<ConfigListener> Listeners { get; set; }

        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        [Obsolete("please use Namespace to configure")]
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

        public string GetNamespace()
        {
            if (Namespace.IsNotNullOrWhiteSpace())
            {
                return Namespace;
            }
#pragma warning disable CS0618
            else if (Tenant.IsNotNullOrWhiteSpace())
            {
                return Tenant;
            }
#pragma warning restore CS0618
            else
            {
                return string.Empty;
            }
        }
    }
}
