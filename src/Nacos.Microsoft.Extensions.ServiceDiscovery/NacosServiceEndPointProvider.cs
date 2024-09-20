#pragma warning disable SA1200
using Nacos.Microsoft.Extensions.ServiceDiscovery;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;
using Nacos.V2;
#pragma warning restore SA1200

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

/// <summary>
/// NacosServiceEndpointProvider
/// </summary>
/// <param name="query">query</param>
/// <param name="hostName">hostName</param>
/// <param name="options">options</param>
/// <param name="logger">logger</param>
/// <param name="namingService">namingService</param>
/// <param name="timeProvider">timeProvider</param>
internal sealed partial class NacosServiceEndpointProvider(
    ServiceEndpointQuery query,
    string hostName,
    IOptionsMonitor<NacosServiceEndpointProviderOptions> options,
    ILogger<NacosServiceEndpointProvider> logger,
    INacosNamingService namingService,
    TimeProvider timeProvider) : NacosServiceEndpointProviderBase(query, logger, timeProvider), IHostNameFeature
{
    protected override double RetryBackOffFactor => options.CurrentValue.RetryBackOffFactor;

    protected override TimeSpan MinRetryPeriod => options.CurrentValue.MinRetryPeriod;

    protected override TimeSpan MaxRetryPeriod => options.CurrentValue.MaxRetryPeriod;

    protected override TimeSpan DefaultRefreshPeriod => options.CurrentValue.DefaultRefreshPeriod;

    string IHostNameFeature.HostName => hostName;

    /// <inheritdoc/>
    public override string ToString() => "Nacos";

    protected override async Task ResolveAsyncCore()
    {
        var endpoints = new List<ServiceEndpoint>();
        var ttl = DefaultRefreshPeriod;
        Log.AddressQuery(logger, ServiceName, hostName);
        var selectInstances = await namingService.SelectInstances(hostName, true).ConfigureAwait(false);
        foreach (var instance in selectInstances)
        {
            var ipAddress = new IPAddress(instance.Ip.Split('.').Select(a => Convert.ToByte(a)).ToArray());
            var ipPoint = new IPEndPoint(ipAddress, instance.Port);
            var serviceEndpoint = ServiceEndpoint.Create(ipPoint);
            serviceEndpoint.Features.Set<IServiceEndpointProvider>(this);
            if (options.CurrentValue.ShouldApplyHostNameMetadata(serviceEndpoint))
            {
                serviceEndpoint.Features.Set<IHostNameFeature>(this);
            }

            endpoints.Add(serviceEndpoint);
        }

        if (endpoints.Count == 0)
        {
            throw new InvalidOperationException($"No records were found for service '{ServiceName}' ( name: '{hostName}').");
        }

        SetResult(endpoints, ttl);
    }
}

