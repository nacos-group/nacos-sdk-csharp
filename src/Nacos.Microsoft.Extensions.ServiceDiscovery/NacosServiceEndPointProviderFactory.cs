#pragma warning disable SA1200
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;
using Nacos.Microsoft.Extensions.ServiceDiscovery;
using Nacos.V2;
#pragma warning restore SA1200

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

/// <summary>
/// NacosServiceEndPointProviderFactory
/// </summary>
/// <param name="options">options</param>
/// <param name="logger">logger</param>
/// <param name="timeProvider">timeProvider</param>
/// <param name="namingService">nacos namingService</param>
internal sealed partial class NacosServiceEndPointProviderFactory(
    IOptionsMonitor<NacosServiceEndpointProviderOptions> options,
    ILogger<NacosServiceEndpointProvider> logger,
    TimeProvider timeProvider,
    INacosNamingService namingService) : IServiceEndpointProviderFactory
{
    /// <summary>
    /// Tries to create an <see cref="T:Microsoft.Extensions.ServiceDiscovery.IServiceEndpointProvider" /> instance for the specified <paramref name="query" />.
    /// </summary>
    /// <param name="query">The service to create the provider for.</param>
    /// <param name="provider">The provider.</param>
    /// <returns><see langword="true" /> if the provider was created, <see langword="false" /> otherwise.</returns>
    public bool TryCreateProvider(ServiceEndpointQuery query, [NotNullWhen(true)] out IServiceEndpointProvider? provider)
    {
        provider = new NacosServiceEndpointProvider(query, query.ServiceName, options, logger, namingService, timeProvider);

        return true;
    }
}
