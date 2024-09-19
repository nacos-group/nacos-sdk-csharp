#pragma warning disable SA1200
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.ServiceDiscovery;
#pragma warning restore SA1200

namespace Nacos.Microsoft.Extensions.ServiceDiscovery;

/// <summary>
/// A service end point provider that uses Nacos to resolve the service end points.
/// </summary>
internal abstract partial class NacosServiceEndpointProviderBase : IServiceEndpointProvider
{
    private readonly object _lock = new();
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly TimeProvider _timeProvider;
    private long _lastRefreshTimeStamp;
    private Task _resolveTask = Task.CompletedTask;
    private bool _hasEndpoints;
    private CancellationChangeToken _lastChangeToken;
    private CancellationTokenSource _lastCollectionCancellation;
    private List<ServiceEndpoint>? _lastEndpointCollection;
    private TimeSpan _nextRefreshPeriod;

    protected NacosServiceEndpointProviderBase(
        ServiceEndpointQuery query,
        ILogger logger,
        TimeProvider timeProvider)
    {
        ServiceName = query.ToString()!;
        _logger = logger;
        _lastEndpointCollection = null;
        _timeProvider = timeProvider;
        _lastRefreshTimeStamp = _timeProvider.GetTimestamp();
        var cancellation = _lastCollectionCancellation = new CancellationTokenSource();
        _lastChangeToken = new CancellationChangeToken(cancellation.Token);
    }

    private TimeSpan ElapsedSinceRefresh => _timeProvider.GetElapsedTime(_lastRefreshTimeStamp);

    protected string ServiceName { get; }

    protected abstract double RetryBackOffFactor { get; }

    protected abstract TimeSpan MinRetryPeriod { get; }

    protected abstract TimeSpan MaxRetryPeriod { get; }

    protected abstract TimeSpan DefaultRefreshPeriod { get; }

    protected CancellationToken ShutdownToken => _disposeCancellation.Token;

    /// <inheritdoc/>
    public async ValueTask PopulateAsync(IServiceEndpointBuilder endpoints, CancellationToken cancellationToken)
    {
        // Only add endpoints to the collection if a previous provider (eg, a configuration override) did not add them.
        if (endpoints.Endpoints.Count != 0)
        {
            Log.SkippedResolution(_logger, ServiceName, "Collection has existing endpoints");
            return;
        }

        if (ShouldRefresh())
        {
            Task resolveTask;
            lock (_lock)
            {
                if (_resolveTask.IsCompleted && ShouldRefresh())
                {
                    _resolveTask = ResolveAsyncCore();
                }

                resolveTask = _resolveTask;
            }

            await resolveTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        lock (_lock)
        {
            if (_lastEndpointCollection is { Count: > 0 } eps)
            {
                foreach (var ep in eps)
                {
                    endpoints.Endpoints.Add(ep);
                }
            }

            endpoints.AddChangeToken(_lastChangeToken);
        }
    }

    private bool ShouldRefresh() => _lastEndpointCollection is null || _lastChangeToken is { HasChanged: true } || ElapsedSinceRefresh >= _nextRefreshPeriod;

    protected abstract Task ResolveAsyncCore();

    protected void SetResult(List<ServiceEndpoint> endpoints, TimeSpan validityPeriod)
    {
        lock (_lock)
        {
            if (endpoints is { Count: > 0 })
            {
                _lastRefreshTimeStamp = _timeProvider.GetTimestamp();
                _nextRefreshPeriod = DefaultRefreshPeriod;
                _hasEndpoints = true;
            }
            else
            {
                _nextRefreshPeriod = GetRefreshPeriod();
                validityPeriod = TimeSpan.Zero;
                _hasEndpoints = false;
            }

            if (validityPeriod <= TimeSpan.Zero)
            {
                validityPeriod = _nextRefreshPeriod;
            }
            else if (validityPeriod > _nextRefreshPeriod)
            {
                validityPeriod = _nextRefreshPeriod;
            }

            _lastCollectionCancellation.Cancel();
            var cancellation = _lastCollectionCancellation = new CancellationTokenSource(validityPeriod, _timeProvider);
            _lastChangeToken = new CancellationChangeToken(cancellation.Token);
            _lastEndpointCollection = endpoints;
        }

        TimeSpan GetRefreshPeriod()
        {
            if (_hasEndpoints)
            {
                return MinRetryPeriod;
            }

            var nextTicks = (long)(_nextRefreshPeriod.Ticks * RetryBackOffFactor);
            if (nextTicks <= 0 || nextTicks > MaxRetryPeriod.Ticks)
            {
                return MaxRetryPeriod;
            }

            return TimeSpan.FromTicks(nextTicks);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        _disposeCancellation.Cancel();

        if (_resolveTask is { } task)
        {
#pragma warning disable CAC002
            await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
#pragma warning restore CAC002
        }
    }
}
