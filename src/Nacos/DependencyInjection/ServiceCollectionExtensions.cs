namespace Nacos.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Nacos;
    using Nacos.Config.Abst;
    using Nacos.Config.FilterImpl;
    using Nacos.Config.Impl;
    using System;
    using System.Net.Http;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNacosV2Config(this IServiceCollection services, Action<NacosSdkOptions> configure, Action<HttpClient> httpClientAction = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.Configure(configure);

            var clientBuilder = services.AddHttpClient(Nacos.Common.Constants.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null)
            {
                clientBuilder.ConfigureHttpClient(httpClientAction);
            }

            services.AddSingleton<INacosConfigService, Config.NacosConfigService>();

            return services;
        }

        public static IServiceCollection AddNacosV2Config(this IServiceCollection services, IConfiguration configuration, Action<HttpClient> httpClientAction = null, string sectionName = "nacos")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Configure<NacosSdkOptions>(configuration.GetSection(sectionName));

            var clientBuilder = services.AddHttpClient(Nacos.Common.Constants.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null)
            {
                clientBuilder.ConfigureHttpClient(httpClientAction);
            }

            services.AddSingleton<INacosConfigService, Config.NacosConfigService>();

            return services;
        }

        public static IServiceCollection AddNacosV2Naming(this IServiceCollection services, Action<NacosSdkOptions> configure, Action<HttpClient> httpClientAction = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.Configure(configure);

            var clientBuilder = services.AddHttpClient(Nacos.Common.Constants.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null) clientBuilder.ConfigureHttpClient(httpClientAction);

            services.AddSingleton<INacosNamingService, Naming.NacosNamingService>();

            return services;
        }

        public static IServiceCollection AddNacosV2Naming(this IServiceCollection services, IConfiguration configuration, Action<HttpClient> httpClientAction = null, string sectionName = "nacos")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Configure<NacosSdkOptions>(configuration.GetSection(sectionName));

            var clientBuilder = services.AddHttpClient(Nacos.Common.Constants.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null) clientBuilder.ConfigureHttpClient(httpClientAction);

            services.AddSingleton<INacosNamingService, Naming.NacosNamingService>();

            return services;
        }
    }
}
