#pragma warning disable SA1200
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ServiceDiscovery;
#pragma warning restore SA1200

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class NacosServiceDiscoveryExtensions
{
    public static IServiceCollection AddNacosSrvServiceEndpointProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddServiceDiscoveryCore();
        services.AddSingleton<IServiceEndpointProviderFactory, NacosServiceEndPointProviderFactory>();
        return services;
    }
}
