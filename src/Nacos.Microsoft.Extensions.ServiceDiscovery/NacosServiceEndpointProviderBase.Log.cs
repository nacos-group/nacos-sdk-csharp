#pragma warning disable SA1200
using Microsoft.Extensions.Logging;
#pragma warning restore SA1200

namespace Nacos.Microsoft.Extensions.ServiceDiscovery;

/// <summary>
/// log base
/// </summary>
internal partial class NacosServiceEndpointProviderBase
{
    /// <summary>
    /// log
    /// </summary>
    internal static partial class Log
    {
        [LoggerMessage(1, LogLevel.Trace, "Resolving endpoints for service '{ServiceName}' using host lookup for name '{RecordName}'.", EventName = "AddressQuery")]
        public static partial void AddressQuery(ILogger logger, string serviceName, string recordName);

        [LoggerMessage(2, LogLevel.Debug, "Skipping endpoint resolution for service '{ServiceName}': '{Reason}'.", EventName = "SkippedResolution")]
        public static partial void SkippedResolution(ILogger logger, string serviceName, string reason);
    }
}
