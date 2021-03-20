namespace Microsoft.AspNetCore.Builder
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Nacos;
    using Nacos.AspNetCore;
    using System;

    [Obsolete("This interface is obsolete and will be removed in a future version.")]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add Nacos AspNetCore.
        /// </summary>
        /// <param name="services">services.</param>
        /// <param name="configuration">configuration</param>
        /// <returns>IServiceCollection</returns>
        [Obsolete("This method is obsolete and will be removed in a future version. Use AddNacosAspNet instead.")]
        public static IServiceCollection AddNacosAspNetCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<NacosAspNetCoreOptions>(configuration.GetSection("nacos"));

            // services.AddNacos(configuration);
            services.AddNacosNaming(configuration);

            services.AddEasyCaching(options =>
            {
                options.UseInMemory(
                    config =>
                    {
                        config.MaxRdSecond = 0;
                    }, "nacos.aspnetcore");
            });

            services.TryAddSingleton<INacosServerManager, NacosServerManager>();

            // load balance strategies
            services.AddSingleton<ILBStrategy, WeightRandomLBStrategy>();
            services.AddSingleton<ILBStrategy, WeightRoundRobinLBStrategy>();

            // IHostedService, report instance status
            services.AddHostedService<StatusReportBgTask>();

            return services;
        }

        /// <summary>
        /// Add Nacos AspNetCore.
        /// </summary>
        /// <param name="services">services.</param>
        /// <param name="nacosAspNetCoreOptions">nacosAspNetCoreOptions.</param>
        /// <param name="nacosOptions">nacosOptions</param>
        /// <returns>IServiceCollection</returns>
        [Obsolete("This method is obsolete and will be removed in a future version. Use AddNacosAspNet instead.")]
        public static IServiceCollection AddNacosAspNetCore(
          this IServiceCollection services,
          Action<NacosAspNetCoreOptions> nacosAspNetCoreOptions,
          Action<NacosOptions> nacosOptions)
        {
            services.Configure(nacosAspNetCoreOptions);
            services.AddNacosNaming(nacosOptions);

            services.AddEasyCaching(options =>
            {
                options.UseInMemory(
                   config =>
                   {
                       config.MaxRdSecond = 0;
                   }, "nacos.aspnetcore");
            });

            services.TryAddSingleton<INacosServerManager, NacosServerManager>();

            // load balance strategies
            services.AddSingleton<ILBStrategy, WeightRandomLBStrategy>();
            services.AddSingleton<ILBStrategy, WeightRoundRobinLBStrategy>();

            services.AddSingleton<ILocalConfigInfoProcessor, MemoryLocalConfigInfoProcessor>();
            services.TryAddSingleton<Nacos.Config.Http.IHttpAgent, Nacos.Config.Http.ServerHttpAgent>();
            services.AddSingleton<INacosConfigClient, NacosConfigClient>();

            // IHostedService, report instance status
            services.AddHostedService<StatusReportBgTask>();

            return services;
        }
    }
}
