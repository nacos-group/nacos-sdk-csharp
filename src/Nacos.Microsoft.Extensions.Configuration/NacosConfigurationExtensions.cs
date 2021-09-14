namespace Microsoft.Extensions.Configuration
{
    using Microsoft.Extensions.Logging;
    using Nacos.Microsoft.Extensions.Configuration;
    using Nacos.V2;
    using System;

    public static class NacosConfigurationExtensions
    {
        /// <summary>
        /// Add Nacos Configuration that integrate with Microsoft.Extensions.Configuration
        /// </summary>
        /// <param name="builder">IConfigurationBuilder</param>
        /// <param name="action">setup NacosConfigurationSource</param>
        /// <returns>IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddNacosV2Configuration(
           this IConfigurationBuilder builder, Action<NacosV2ConfigurationSource> action)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var source = new NacosV2ConfigurationSource();

            action(source);

            source.NacosConfigurationParser ??= DefaultJsonConfigurationStringParser.Instance;

            return builder.Add(source);
        }

        /// <summary>
        /// Add Nacos Configuration that integrate with Microsoft.Extensions.Configuration
        /// </summary>
        /// <param name="builder">IConfigurationBuilder</param>
        /// <param name="configuration">Configuration binding nacos configuration source</param>
        /// <param name="parser">The parser.</param>
        /// <param name="logAction">The logging action.</param>
        /// <returns>IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddNacosV2Configuration(
           this IConfigurationBuilder builder,
           IConfiguration configuration,
           INacosConfigurationParser parser = null,
           Action<ILoggingBuilder> logAction = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var source = new NacosV2ConfigurationSource();
            configuration.Bind(source);
            source.NacosConfigurationParser = parser ?? DefaultJsonConfigurationStringParser.Instance;
            source.LoggingBuilder = logAction;

            return builder.Add(source);
        }
    }
}
