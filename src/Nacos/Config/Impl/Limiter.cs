namespace Nacos.Config.Impl
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Nacos.Config.Impl.RateLimiter;
    using Nacos.Logging;

    public class Limiter
    {
        private static readonly ILogger _logger = NacosLogManager.CreateLogger<ClientWorker>();

        private static readonly int LIMIT_TIME = 1000;

        private static int _limit = 5;

        private static ConcurrentDictionary<string, TokenBucketRateLimiter> _cache = new ConcurrentDictionary<string, TokenBucketRateLimiter>();

        public static async Task<bool> IsLimitAsync(string accessKeyID)
        {
            var exist = _cache.TryGetValue(accessKeyID, out var rateLimiter);

            TokenBucketLease lease = null;
            if (exist)
            {
                lease = await rateLimiter.AcquireAsync().ConfigureAwait(false);
            }
            else
            {
                var rlOption = new TokenBucketRateLimiterOptions
                {
                    ReplenishmentPeriod = TimeSpan.FromMilliseconds(LIMIT_TIME),
                    TokensPerPeriod = 5,
                    TokenLimit = _limit,
                };
                rateLimiter = new TokenBucketRateLimiter(rlOption);
                lease = await rateLimiter.AcquireAsync().ConfigureAwait(false);
                _cache.TryAdd(accessKeyID, rateLimiter);
            }

            var isLimit = !lease.IsAcquired;
            lease.Dispose();

            if (isLimit)
                _logger.LogError("access_key_id:{} limited", accessKeyID);

            return isLimit;
        }
    }
}
