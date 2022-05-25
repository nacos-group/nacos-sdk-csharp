namespace Microsoft.Extensions.Configuration
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
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
        /// <param name="client">The nacos config client</param>
        /// <param name="loggerFactory">The loggerFactory</param>
        /// <param name="parser">The parser.</param>
        /// <param name="logAction">The logAction.</param>
        /// <returns>IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddNacosV2Configuration(
           this IConfigurationBuilder builder,
           Action<NacosV2ConfigurationSource> action,
           INacosConfigService client = null,
           ILoggerFactory loggerFactory = null,
           INacosConfigurationParser parser = null,
           Action<ILoggingBuilder> logAction = null)
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
            source.NacosConfigurationParser ??= parser ?? DefaultJsonConfigurationStringParser.Instance;

            BuildDISource(source, client, loggerFactory, logAction);

            return builder.Add(source);
        }

        /// <summary>
        /// Add Nacos Configuration that integrate with Microsoft.Extensions.Configuration
        /// </summary>
        /// <param name="builder">IConfigurationBuilder</param>
        /// <param name="configuration">Configuration binding nacos configuration source</param>
        /// <param name="client">The nacos config client</param>
        /// <param name="loggerFactory">The loggerFactory</param>
        /// <param name="parser">The parser.</param>
        /// <param name="logAction">The logAction.</param>
        /// <returns>IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddNacosV2Configuration(
           this IConfigurationBuilder builder,
           IConfiguration configuration,
           INacosConfigService client = null,
           ILoggerFactory loggerFactory = null,
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

            var source = new NacosV2ConfigurationSource(null, null);
            configuration.Bind(source);
            source.NacosConfigurationParser = parser ?? DefaultJsonConfigurationStringParser.Instance;

            BuildDISource(source, client, loggerFactory, logAction);

            return builder.Add(source);
        }

        /// <summary>
        /// Use nacos config combine IHostBuilder and ConfigureAppConfiguration
        /// </summary>
        /// <param name="builder">host builder.</param>
        /// <param name="section">basic nacos configuration section.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="logAction">The logAction.</param>
        /// <returns>IHostBuilder</returns>
        public static IHostBuilder UseNacosConfig(this IHostBuilder builder, string section, INacosConfigurationParser parser = null, Action<ILoggingBuilder> logAction = null)
        {
            builder.ConfigureAppConfiguration((_, cfb) =>
            {
                var config = cfb.Build();

                cfb.AddNacosV2Configuration(config.GetSection(section), parser: parser, logAction: logAction);
            });

            return builder;
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// Use nacos config combine IWebHostBuilder and ConfigureAppConfiguration
        /// </summary>
        /// <param name="builder">host builder.</param>
        /// <param name="section">basic nacos configuration section.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="logAction">The logAction.</param>
        /// <returns>IHostBuilder</returns>
        public static AspNetCore.Hosting.IWebHostBuilder UseNacosConfig(this AspNetCore.Hosting.IWebHostBuilder builder, string section, INacosConfigurationParser parser = null, Action<ILoggingBuilder> logAction = null)
        {
            builder.ConfigureAppConfiguration((_, cfb) =>
            {
                var config = cfb.Build();

                cfb.AddNacosV2Configuration(config.GetSection(section), parser: parser, logAction: logAction);
            });

            return builder;
        }
#endif

        private static void BuildDISource(
            NacosV2ConfigurationSource source,
            INacosConfigService client,
            ILoggerFactory logFactory,
            Action<ILoggingBuilder> logAction)
        {
            if (client == null)
            {
                IServiceCollection serviceCollection = new ServiceCollection();

                var sdkAction = source.GetNacosSdkOptions();
                serviceCollection.AddNacosV2Config(sdkAction);
                serviceCollection.AddLogging(logAction ?? (x => x.AddConsole()));

                var serviceProvider = serviceCollection.BuildServiceProvider();

                client = serviceProvider.GetService<INacosConfigService>();
                logFactory = serviceProvider.GetService<ILoggerFactory>();
            }

            source.Client = client ?? throw new Nacos.V2.Exceptions.NacosException("Can't get INacosConfigService instance from DI Container");
            source.LoggerFactory = logFactory;
        }
    }
}
