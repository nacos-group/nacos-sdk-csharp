#pragma warning disable SA1200
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;
#pragma warning restore SA1200

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class NacosServiceDiscoveryExtensions
{
    /// <summary>
    /// Configures a service discovery endpoint provider which uses <see cref="T:Nacos.V2.INacosNamingService" /> to resolve endpoints.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNacosServiceEndpointProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddServiceDiscoveryCore();
        services.AddSingleton<IServiceEndpointProviderFactory, NacosServiceEndPointProviderFactory>();
        return services;
    }
}
