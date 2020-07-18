namespace Microsoft.Extensions.Configuration
{
    using Nacos.Microsoft.Extensions.Configuration;
    using System;

    public static class NacosConfigurationExtensions
    {
        /// <summary>
        /// Add Nacos Configuration
        /// </summary>
        /// <param name="builder">IConfigurationBuilder</param>
        /// <param name="action">setup NacosConfigurationSource</param>
        /// <returns>IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddNacosConfiguration(
           this IConfigurationBuilder builder, Action<NacosConfigurationSource> action)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var source = new NacosConfigurationSource();

            action(source);

            return builder.Add(source);
        }

        /// <summary>
        /// Add Nacos Configuration
        /// </summary>
        /// <param name="builder">IConfigurationBuilder</param>
        /// <param name="configuration">Configuration binding nacos configuration source</param>
        /// <returns>IConfigurationBuilder</returns>
        public static IConfigurationBuilder AddNacosConfiguration(
           this IConfigurationBuilder builder, IConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var source = new NacosConfigurationSource();

            configuration.Bind(source);

            return builder.Add(source);
        }
    }
}
