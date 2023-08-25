namespace Nacos.DependencyInjection
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


/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
            var clientBuilder = services.AddHttpClient(V2.Common.Constants.ClientName)
在此之后:
            var clientBuilder = services.AddHttpClient(Constants.ClientName)
*/
            var clientBuilder = services.AddHttpClient(Nacos.Common.Constants.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null)
            {
                clientBuilder.ConfigureHttpClient(httpClientAction);
            }


            /* 项目“Nacos (netstandard2.0)”的未合并的更改
            在此之前:
                        services.AddSingleton<Nacos.INacosConfigService, Nacos.Config.NacosConfigService>();
            在此之后:
                        services.AddSingleton<Nacos.INacosConfigService, NacosConfigService>();
            */
            services.AddSingleton<INacosConfigService, Config.NacosConfigService>();

            return services;
        }

        public static IServiceCollection AddNacosV2Config(this IServiceCollection services, IConfiguration configuration, Action<HttpClient> httpClientAction = null, string sectionName = "nacos")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Configure<NacosSdkOptions>(configuration.GetSection(sectionName));


/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
            var clientBuilder = services.AddHttpClient(V2.Common.Constants.ClientName)
在此之后:
            var clientBuilder = services.AddHttpClient(Constants.ClientName)
*/
            var clientBuilder = services.AddHttpClient(Nacos.Common.Constants.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null)
            {
                clientBuilder.ConfigureHttpClient(httpClientAction);
            }


            /* 项目“Nacos (netstandard2.0)”的未合并的更改
            在此之前:
                        services.AddSingleton<Nacos.INacosConfigService, Nacos.Config.NacosConfigService>();
            在此之后:
                        services.AddSingleton<Nacos.INacosConfigService, NacosConfigService>();
            */
            services.AddSingleton<INacosConfigService, Config.NacosConfigService>();

            return services;
        }

        public static IServiceCollection AddNacosV2Naming(this IServiceCollection services, Action<NacosSdkOptions> configure, Action<HttpClient> httpClientAction = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.Configure(configure);


/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
            var clientBuilder = services.AddHttpClient(V2.Common.Constants.ClientName)
在此之后:
            var clientBuilder = services.AddHttpClient(Constants.ClientName)
*/
            var clientBuilder = services.AddHttpClient(Nacos.Common.Constants.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null) clientBuilder.ConfigureHttpClient(httpClientAction);


            /* 项目“Nacos (netstandard2.0)”的未合并的更改
            在此之前:
                        services.AddSingleton<INacosNamingService, Nacos.Naming.NacosNamingService>();
            在此之后:
                        services.AddSingleton<INacosNamingService, NacosNamingService>();
            */
            services.AddSingleton<INacosNamingService, Naming.NacosNamingService>();

            return services;
        }

        public static IServiceCollection AddNacosV2Naming(this IServiceCollection services, IConfiguration configuration, Action<HttpClient> httpClientAction = null, string sectionName = "nacos")
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.Configure<NacosSdkOptions>(configuration.GetSection(sectionName));


/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
            var clientBuilder = services.AddHttpClient(V2.Common.Constants.ClientName)
在此之后:
            var clientBuilder = services.AddHttpClient(Constants.ClientName)
*/
            var clientBuilder = services.AddHttpClient(Nacos.Common.Constants.ClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null) clientBuilder.ConfigureHttpClient(httpClientAction);


            /* 项目“Nacos (netstandard2.0)”的未合并的更改
            在此之前:
                        services.AddSingleton<INacosNamingService, Nacos.Naming.NacosNamingService>();
            在此之后:
                        services.AddSingleton<INacosNamingService, NacosNamingService>();
            */
            services.AddSingleton<INacosNamingService, Naming.NacosNamingService>();

            return services;
        }
    }
}
