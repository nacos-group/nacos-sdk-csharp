namespace Nacos.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Nacos;
    using System;
    using System.Net.Http;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNacosV2Config(this IServiceCollection services, Action<NacosOptions> configure, Action<HttpClient> httpClientAction = null)
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

            services.AddSingleton<Nacos.INacosConfigClient, Nacos.Config.GrpcConfigClient>();
            services.AddSingleton<INacosConfigClientFactory, NacosConfigClientFactory>();
            services.AddSingleton<Nacos.Config.Abst.IConfigTransportClient, Nacos.Config.Impl.ConfiggRpcTransportClient>();
            services.AddSingleton<Nacos.Security.V2.ISecurityProxy, Nacos.Security.V2.SecurityProxy>();

            return services;
        }

        public static IServiceCollection AddNacosConfig(this IServiceCollection services, IConfiguration configuration, Action<HttpClient> httpClientAction = null, string sectionName = "nacos")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Configure<NacosOptions>(configuration.GetSection(sectionName));

            var clientBuilder = services.AddHttpClient(ConstValue.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false });

            if (httpClientAction != null)
            {
                clientBuilder.ConfigureHttpClient(httpClientAction);
            }

            services.AddSingleton<Nacos.INacosConfigClient, Nacos.Config.GrpcConfigClient>();
            services.AddSingleton<INacosConfigClientFactory, NacosConfigClientFactory>();
            services.AddSingleton<Nacos.Config.Abst.IConfigTransportClient, Nacos.Config.Impl.ConfiggRpcTransportClient>();
            services.AddSingleton<Nacos.Security.V2.ISecurityProxy, Nacos.Security.V2.SecurityProxy>();

            return services;
        }
    }
}
