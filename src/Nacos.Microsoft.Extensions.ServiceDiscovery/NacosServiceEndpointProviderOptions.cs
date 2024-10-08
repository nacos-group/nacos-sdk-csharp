﻿#pragma warning disable SA1200
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ServiceDiscovery;
#pragma warning restore SA1200

namespace Nacos.Microsoft.Extensions.ServiceDiscovery;

/// <summary>
/// Options for configuring <see cref="NacosServiceEndpointProvider"/>.
/// </summary>
public class NacosServiceEndpointProviderOptions
{
    /// <summary>
    /// Gets or sets the default refresh period for endpoints resolved from Nacos.
    /// </summary>
    public TimeSpan DefaultRefreshPeriod { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the initial period between retries.
    /// </summary>
    public TimeSpan MinRetryPeriod { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum period between retries.
    /// </summary>
    public TimeSpan MaxRetryPeriod { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the retry period growth factor.
    /// </summary>
    public double RetryBackOffFactor { get; set; } = 2;

    /// <summary>
    /// Gets or sets a delegate used to determine whether to apply host name metadata to each resolved endpoint. Defaults to <c>false</c>.
    /// </summary>
    public Func<ServiceEndpoint, bool> ShouldApplyHostNameMetadata { get; set; } = _ => false;
}
