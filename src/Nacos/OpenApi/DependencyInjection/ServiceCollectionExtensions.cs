namespace Nacos.OpenApi
{
    using Microsoft.Extensions.DependencyInjection;
    using Nacos.V2;
    using System;
    using System.Net.Http;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNacosOpenApi(this IServiceCollection services, Action<NacosSdkOptions> configure, Action<HttpClient> httpClientAction = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            services.Configure(configure);

            var clientBuilder = services.AddHttpClient(Constants.HttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { UseProxy = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });

            if (httpClientAction != null)
            {
                clientBuilder.ConfigureHttpClient(httpClientAction);
            }

            services.AddSingleton<INacosOpenApi, DefaultNacosOpenApi>();

            return services;
        }
    }
}
