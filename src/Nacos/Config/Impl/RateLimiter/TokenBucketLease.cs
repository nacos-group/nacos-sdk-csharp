namespace Nacos.Config.Impl.RateLimiter
{
    using System;

    public class TokenBucketLease : IDisposable
    {
        private readonly TimeSpan? _retryAfter;

        public TokenBucketLease(bool isAcquired, TimeSpan? retryAfter)
        {
            IsAcquired = isAcquired;
            _retryAfter = retryAfter;
        }

        public bool IsAcquired { get; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
