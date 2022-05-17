namespace Microsoft.Extensions.Configuration
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Nacos.Microsoft.Extensions.Configuration;
    using Nacos.V2;
    using Nacos.V2.DependencyInjection;
    using System;

    public static class NacosConfigurationExtensions
    {
        /// <summary>
        /// Add Nacos Configuration that integrate with Microsoft.Extensions.Configuration
        /// </summary>
        /// <param name="builder">IConfigurationBuilder</param>
        /// <param name="action">setup NacosConfigurationSource</param>
        /// <param name="serviceCollection">setup NacosConfigurationSource</param>
        /// <returns>IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddNacosV2Configuration(
           this IConfigurationBuilder builder, Action<NacosV2ConfigurationSource> action, IServiceCollection serviceCollection)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var source = new NacosV2ConfigurationSource(null, null);
            action.Invoke(source);
            source.NacosConfigurationParser ??= DefaultJsonConfigurationStringParser.Instance;

            BuildDISource(source, serviceCollection);

            return builder.Add(source);
        }

        /// <summary>
        /// Add Nacos Configuration that integrate with Microsoft.Extensions.Configuration
        /// </summary>
        /// <param name="builder">IConfigurationBuilder</param>
        /// <param name="configuration">Configuration binding nacos configuration source</param>
        /// <param name="serviceCollection">setup NacosConfigurationSource</param>
        /// <param name="parser">The parser.</param>
        /// <returns>IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddNacosV2Configuration(
           this IConfigurationBuilder builder,
           IConfiguration configuration,
           IServiceCollection serviceCollection,
           INacosConfigurationParser parser = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var source = new NacosV2ConfigurationSource(null, null);
            configuration.Bind(source);
            source.NacosConfigurationParser = parser ?? DefaultJsonConfigurationStringParser.Instance;

            BuildDISource(source, serviceCollection);

            return builder.Add(source);
        }

        private static void BuildDISource(NacosV2ConfigurationSource source, IServiceCollection serviceCollection)
        {
            var sdkAction = source.GetNacosSdkOptions();
            serviceCollection.AddNacosV2Config(sdkAction);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var client = serviceProvider.GetService<INacosConfigService>();
            var logFactory = serviceProvider.GetService<ILoggerFactory>();

            source.Client = client ?? throw new Nacos.V2.Exceptions.NacosException("Can't get INacosConfigService instance from DI Container");
            source.LoggerFactory = logFactory;
            source.NacosConfigurationParser ??= DefaultJsonConfigurationStringParser.Instance;
        }
    }
}
