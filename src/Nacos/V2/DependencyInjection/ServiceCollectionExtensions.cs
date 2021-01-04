namespace Nacos.V2.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Nacos;
    using System;
    using System.Net.Http;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNacosV2Config(this IServiceCollection services, Action<NacosSdkOptions> configure, Action<HttpClient> httpClientAction = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.Configure(configure);

            var clientBuilder = services.AddHttpClient(ConstValue.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });

            if (httpClientAction != null)
            {
                clientBuilder.ConfigureHttpClient(httpClientAction);
            }

            services.AddSingleton<Nacos.INacosConfigClient, Nacos.V2.Config.GrpcConfigClient>();
            services.AddSingleton<INacosConfigClientFactory, NacosConfigClientFactory>();
            services.AddSingleton<Nacos.V2.Config.Abst.IConfigTransportClient, Nacos.V2.Config.Impl.ConfiggRpcTransportClient>();
            services.AddSingleton<Nacos.V2.Security.ISecurityProxy, Nacos.V2.Security.SecurityProxy>();

            return services;
        }

        public static IServiceCollection AddNacosV2Config(this IServiceCollection services, IConfiguration configuration, Action<HttpClient> httpClientAction = null, string sectionName = "nacos")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Configure<NacosSdkOptions>(configuration.GetSection(sectionName));

            var clientBuilder = services.AddHttpClient(ConstValue.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });

            if (httpClientAction != null)
            {
                clientBuilder.ConfigureHttpClient(httpClientAction);
            }

            services.AddSingleton<Nacos.INacosConfigClient, Nacos.V2.Config.GrpcConfigClient>();
            services.AddSingleton<INacosConfigClientFactory, NacosConfigClientFactory>();
            services.AddSingleton<Nacos.V2.Config.Abst.IConfigTransportClient, Nacos.V2.Config.Impl.ConfiggRpcTransportClient>();
            services.AddSingleton<Nacos.V2.Security.ISecurityProxy, Nacos.V2.Security.SecurityProxy>();

            return services;
        }

        public static IServiceCollection AddNacosV2Naming(this IServiceCollection services, Action<NacosSdkOptions> configure, Action<HttpClient> httpClientAction = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.Configure(configure);

            var clientBuilder = services.AddHttpClient(ConstValue.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });

            if (httpClientAction != null) clientBuilder.ConfigureHttpClient(httpClientAction);

            services.AddSingleton<INacosNamingService, Nacos.V2.Naming.NacosNamingService>();

            return services;
        }

        public static IServiceCollection AddNacosV2Naming(this IServiceCollection services, IConfiguration configuration, Action<HttpClient> httpClientAction = null, string sectionName = "nacos")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Configure<NacosSdkOptions>(configuration.GetSection(sectionName));

            var clientBuilder = services.AddHttpClient(ConstValue.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });

            if (httpClientAction != null) clientBuilder.ConfigureHttpClient(httpClientAction);

            services.AddSingleton<INacosNamingService, Nacos.V2.Naming.NacosNamingService>();

            return services;
        }
    }
}
