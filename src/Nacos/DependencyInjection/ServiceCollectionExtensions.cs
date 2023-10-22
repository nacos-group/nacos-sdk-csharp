namespace Nacos.DependencyInjection
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Nacos;
    using Nacos.Auth;
    using Nacos.Config;
    using Nacos.Config.Core;
    using Nacos.Config.Filter;
    using Nacos.Config.Remote;
    using Nacos.Config.Remote.Grpc;
    using Nacos.Naming.Cache;
    using Nacos.Naming.Event;
    using Nacos.Naming.Remote;
    using Nacos.Naming.Remote.Grpc;
    using Nacos.Naming.Remote.Http;
    using Nacos.Remote;
    using Nacos.Security;
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

            services.TryAddSingleton<IClientAuthService, NacosClientAuthServiceImpl>();
            services.AddSingleton<IClientWorker, ClientWorker>();
            services.AddSingleton<IConfigFilterChain, ConfigFilterChainManager>();
            services.AddSingleton<IConfigTransportClient, ConfigRpcTransportClient>();
            services.AddSingleton<IServerListFactory, ServerListManager>();
            services.AddSingleton<ISecurityProxy, SecurityProxy>();
            services.AddSingleton<INacosConfigService, NacosConfigService>();

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

            services.AddSingleton<IConfigFilterChain, ConfigFilterChainManager>();

            services.AddSingleton<INacosConfigService, NacosConfigService>();

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

            services.TryAddSingleton<IClientAuthService, NacosClientAuthServiceImpl>();
            services.AddSingleton<InstancesChangeNotifier>();
            services.AddSingleton<ServiceInfoHolder>();
            services.AddSingleton<ISecurityProxy, SecurityProxy>();
            services.AddSingleton<IServerListFactory, Nacos.Remote.ServerListManager>();
            services.AddSingleton<INamingHttpClientProxy, NamingHttpClientProxy>();
            services.AddSingleton<INamingGrpcClientProxy, NamingGrpcClientProxy>();
            services.AddSingleton<INamingClientProxy, NamingClientProxyDelegate>();
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

            services.TryAddSingleton<IClientAuthService, NacosClientAuthServiceImpl>();
            services.AddSingleton<InstancesChangeNotifier>();
            services.AddSingleton<ServiceInfoHolder>();
            services.AddSingleton<ISecurityProxy, SecurityProxy>();
            services.AddSingleton<IServerListFactory, Nacos.Remote.ServerListManager>();
            services.AddSingleton<INamingHttpClientProxy, NamingHttpClientProxy>();
            services.AddSingleton<INamingGrpcClientProxy, NamingGrpcClientProxy>();
            services.AddSingleton<INamingClientProxy, NamingClientProxyDelegate>();
            services.AddSingleton<INacosNamingService, Naming.NacosNamingService>();

            return services;
        }
    }
}
