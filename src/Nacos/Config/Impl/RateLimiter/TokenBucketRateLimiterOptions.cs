namespace Nacos.Config.Impl.RateLimiter
{
    using System;

    public class TokenBucketRateLimiterOptions
    {
        public TimeSpan ReplenishmentPeriod { get; set; } = TimeSpan.Zero;

        public int TokensPerPeriod { get; set; }

        public bool AutoReplenishment { get; set; } = true;

        public int TokenLimit { get; set; }
    }
}
