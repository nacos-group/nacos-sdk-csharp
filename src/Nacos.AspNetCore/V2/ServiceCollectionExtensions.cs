namespace Nacos.AspNetCore.V2
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Nacos.V2.DependencyInjection;


    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNacosAspNet(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<NacosAspNetOptions>(configuration.GetSection("nacos"));

            services.AddNacosV2Naming(configuration);

            services.AddHostedService<RegSvcBgTask>();

            return services;
        }
    }
}
