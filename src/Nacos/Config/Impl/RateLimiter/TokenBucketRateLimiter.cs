namespace Nacos.Config.Impl.RateLimiter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class TokenBucketRateLimiter : IAsyncDisposable, IDisposable
    {
        private double _tokenCount;
        private int _queueCount;
        private long _lastReplenishmentTick;
        private long? _idleSince;
        private bool _disposed;

        private long _failedLeasesCount;
        private long _successfulLeasesCount;

        private readonly double _fillRate;
        private readonly Timer _renewTimer;
        private readonly TokenBucketRateLimiterOptions _options;
        private readonly Queue<RequestRegistration> _queue = new Queue<RequestRegistration>();

        // Use the queue as the lock field so we don't need to allocate another object for a lock and have another field in the object
        private object Lock => _queue;

        private static readonly TokenBucketLease SuccessfulLease = new TokenBucketLease(true, null);
        private static readonly TokenBucketLease FailedLease = new TokenBucketLease(false, null);
        private static readonly double TickFrequency = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

        public bool IsAutoReplenishing => _options.AutoReplenishment;

        public TimeSpan ReplenishmentPeriod => _options.ReplenishmentPeriod;


        public TokenBucketRateLimiter(TokenBucketRateLimiterOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.TokenLimit <= 0)
            {
                throw new ArgumentException($"{nameof(options.TokenLimit)} must be set to a value greater than 0.", nameof(options));
            }

            if (options.TokensPerPeriod <= 0)
            {
                throw new ArgumentException($"{nameof(options.TokensPerPeriod)} must be set to a value greater than 0.", nameof(options));
            }

            if (options.ReplenishmentPeriod <= TimeSpan.Zero)
            {
                throw new ArgumentException($"{nameof(options.ReplenishmentPeriod)} must be set to a value greater than TimeSpan.Zero.", nameof(options));
            }

            _options = new TokenBucketRateLimiterOptions
            {
                TokenLimit = options.TokenLimit,
                ReplenishmentPeriod = options.ReplenishmentPeriod,
                TokensPerPeriod = options.TokensPerPeriod,
                AutoReplenishment = options.AutoReplenishment
            };

            _tokenCount = options.TokenLimit;
            _fillRate = (double)options.TokensPerPeriod / options.ReplenishmentPeriod.Ticks;

            _idleSince = _lastReplenishmentTick = Stopwatch.GetTimestamp();

            if (_options.AutoReplenishment)
            {
                _renewTimer = new Timer(Replenish, this, _options.ReplenishmentPeriod, _options.ReplenishmentPeriod);
            }
        }

        public object GetStatistics()
        {
            ThrowIfDisposed();
            return new
            {
                CurrentAvailablePermits = (long)_tokenCount,
                CurrentQueuedCount = _queueCount,
                TotalFailedLeases = Interlocked.Read(ref _failedLeasesCount),
                TotalSuccessfulLeases = Interlocked.Read(ref _successfulLeasesCount),
            };
        }

        public ValueTask<TokenBucketLease> AcquireAsync(int tokenCount = 1, CancellationToken cancellationToken = default)
        {
            // These amounts of resources can never be acquired
            if (tokenCount > _options.TokenLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(tokenCount), tokenCount, $"{tokenCount} token(s) exceeds the token limit of {_options.TokenLimit}.");
            }

            ThrowIfDisposed();

            // Return SuccessfulAcquisition if requestedCount is 0 and resources are available
            if (tokenCount == 0 && _tokenCount > 0)
            {
                Interlocked.Increment(ref _successfulLeasesCount);
                return new ValueTask<TokenBucketLease>(SuccessfulLease);
            }

            using var disposer = default(RequestRegistration.Disposer);
            lock (Lock)
            {
                if (TryLeaseUnsynchronized(tokenCount, out TokenBucketLease lease))
                {
                    return new ValueTask<TokenBucketLease>(lease);
                }

                var registration = new RequestRegistration(tokenCount, this, cancellationToken);
                _queue.Enqueue(registration);
                _queueCount += tokenCount;

                return new ValueTask<TokenBucketLease>(registration.Task);
            }
        }

        private TokenBucketLease CreateFailedTokenLease(int tokenCount)
        {
            int replenishAmount = tokenCount - (int)_tokenCount + _queueCount;

            // can't have 0 replenish periods, that would mean it should be a successful lease
            // if TokensPerPeriod is larger than the replenishAmount needed then it would be 0
            int replenishPeriods = Math.Max(replenishAmount / _options.TokensPerPeriod, 1);

            return new TokenBucketLease(false, TimeSpan.FromTicks(_options.ReplenishmentPeriod.Ticks * replenishPeriods));
        }

        private bool TryLeaseUnsynchronized(int tokenCount, out TokenBucketLease lease)
        {
            ThrowIfDisposed();

            // if permitCount is 0 we want to queue it if there are no available permits
            if (_tokenCount >= tokenCount && _tokenCount != 0)
            {
                if (tokenCount == 0)
                {
                    Interlocked.Increment(ref _successfulLeasesCount);

                    // Edge case where the check before the lock showed 0 available permits but when we got the lock some permits were now available
                    lease = SuccessfulLease;
                    return true;
                }

                // if there are no items queued we can lease
                if (_queueCount == 0)
                {
                    _idleSince = null;
                    _tokenCount -= tokenCount;

                    Interlocked.Increment(ref _successfulLeasesCount);
                    lease = SuccessfulLease;
                    return true;
                }
            }

            lease = null;
            return false;
        }


        private static void Replenish(object state)
        {
            TokenBucketRateLimiter limiter = (state as TokenBucketRateLimiter)!;

            // Use Stopwatch instead of DateTime.UtcNow to avoid issues on systems where the clock can change
            long nowTicks = Stopwatch.GetTimestamp();
            limiter!.ReplenishInternal(nowTicks);
        }

        // Used in tests to avoid dealing with real time
        private void ReplenishInternal(long nowTicks)
        {
            using var disposer = default(RequestRegistration.Disposer);

            // method is re-entrant (from Timer), lock to avoid multiple simultaneous replenishes
            lock (Lock)
            {
                if (_disposed)
                {
                    return;
                }

                if (_tokenCount == _options.TokenLimit)
                {
                    return;
                }

                double add;

                // Trust the timer to be close enough to when we want to replenish, this avoids issues with Timer jitter where it might be .99 seconds instead of 1, and 1.1 seconds the next time etc.
                if (_options.AutoReplenishment)
                {
                    add = _options.TokensPerPeriod;
                }
                else
                {
                    add = _fillRate * (nowTicks - _lastReplenishmentTick) * TickFrequency;
                }

                _tokenCount = Math.Min(_options.TokenLimit, _tokenCount + add);

                _lastReplenishmentTick = nowTicks;

                // Process queued requests
                Queue<RequestRegistration> queue = _queue;

                while (queue.Count > 0)
                {
                    RequestRegistration nextPendingRequest = queue.Peek();

                    // Request was handled already, either via cancellation or being kicked from the queue due to a newer request being queued.
                    // We just need to remove the item and let the next queued item be considered for completion.
                    if (nextPendingRequest.Task.IsCompleted)
                    {
                        nextPendingRequest = queue.Dequeue();
                        disposer.Add(nextPendingRequest);
                    }
                    else if (_tokenCount >= nextPendingRequest.Count)
                    {
                        // Request can be fulfilled
                        nextPendingRequest = queue.Dequeue();

                        _queueCount -= nextPendingRequest.Count;
                        _tokenCount -= nextPendingRequest.Count;

                        if (!nextPendingRequest.TrySetResult(SuccessfulLease))
                        {
                            // Queued item was canceled so add count back
                            _tokenCount += nextPendingRequest.Count;

                            // Updating queue count is handled by the cancellation code
                            _queueCount += nextPendingRequest.Count;
                        }
                        else
                        {
                            Interlocked.Increment(ref _successfulLeasesCount);
                        }

                        disposer.Add(nextPendingRequest);
                    }
                    else
                    {
                        // Request cannot be fulfilled
                        break;
                    }
                }

                if (_tokenCount == _options.TokenLimit)
                {
                    _idleSince = Stopwatch.GetTimestamp();
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TokenBucketRateLimiter));
            }
        }

        public void Dispose()
        {
            using var disposer = default(RequestRegistration.Disposer);
            lock (Lock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _renewTimer?.Dispose();
                while (_queue.Count > 0)
                {
                    RequestRegistration next = _queue.Dequeue();
                    disposer.Add(next);
                    next.TrySetResult(FailedLease);
                }
            }

            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();

            return default;
        }

        protected class RequestRegistration : TaskCompletionSource<TokenBucketLease>
        {
            private readonly CancellationToken _cancellationToken;
            private CancellationTokenRegistration _cancellationTokenRegistration;

            // this field is used only by the disposal mechanics and never shared between threads
            private RequestRegistration _next;

            public RequestRegistration(int permitCount, TokenBucketRateLimiter limiter, CancellationToken cancellationToken)
                : base(limiter, TaskCreationOptions.RunContinuationsAsynchronously)
            {
                Count = permitCount;
                _cancellationToken = cancellationToken;

                // RequestRegistration objects are created while the limiter lock is held
                // if cancellationToken fires before or while the lock is held, UnsafeRegister
                // is going to invoke the callback synchronously, but this does not create
                // a deadlock because lock are reentrant
                if (cancellationToken.CanBeCanceled)
                    _cancellationTokenRegistration = cancellationToken.Register(Cancel, this);
            }

            public int Count { get; }

            private static void Cancel(object state)
            {
                if (state is RequestRegistration registration && registration.TrySetCanceled(registration._cancellationToken))
                {
                    var limiter = (TokenBucketRateLimiter)registration.Task.AsyncState!;
                    lock (limiter.Lock)
                    {
                        limiter._queueCount -= registration.Count;
                    }
                }
            }

            /// <summary>
            /// Collects registrations to dispose outside the limiter lock to avoid deadlock.
            /// </summary>
            public struct Disposer : IDisposable
            {
                private RequestRegistration _next;

                public void Add(RequestRegistration request)
                {
                    request._next = _next;
                    _next = request;
                }

                public void Dispose()
                {
                    for (var current = _next; current is not null; current = current._next)
                    {
                        current._cancellationTokenRegistration.Dispose();
                    }

                    _next = null;
                }
            }
        }
    }
}
