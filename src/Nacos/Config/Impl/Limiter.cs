namespace Nacos.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Nacos.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.RateLimiting;
    using System.Threading.Tasks;

    public class Limiter
    {
        private static readonly ILogger _logger = NacosLogManager.CreateLogger<ClientWorker>();

        private static readonly int LIMIT_TIME = 1000;

        private static int _limit = 5;

        private static ConcurrentDictionary<string, RateLimiter> _cache = new ConcurrentDictionary<string, RateLimiter>();

        public static bool IsLimit(string accessKeyID)
        {
            var exist = _cache.TryGetValue(accessKeyID, out var rateLimiter);

            RateLimitLease lease = null;
            if (exist)
            {
                lease = rateLimiter.AttemptAcquire();
            }
            else
            {
                var rlOption = new TokenBucketRateLimiterOptions
                {
                    ReplenishmentPeriod = TimeSpan.FromMilliseconds(LIMIT_TIME),
                    TokensPerPeriod = 5,
                    TokenLimit = _limit
                };
                rateLimiter = new TokenBucketRateLimiter(rlOption);
                lease = rateLimiter.AttemptAcquire();
                _cache.TryAdd(accessKeyID, rateLimiter);
            }

            var isLimit = lease.IsAcquired;
            lease.Dispose();

            if (!isLimit)
                _logger.LogError("access_key_id:{} limited", accessKeyID);

            return isLimit;
        }
    }
}
