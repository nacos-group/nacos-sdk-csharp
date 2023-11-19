namespace Nacos.Tests.Config.Impl
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Nacos.Config.Impl;
    using Xunit;
    using Xunit.Abstractions;

    public class LimiterTests
    {
        private readonly ITestOutputHelper _output;

        public LimiterTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task IsLimit_Should_Succeed()
        {
            var accessKeyID = "a";
            Assert.False(await Limiter.IsLimitAsync(accessKeyID).ConfigureAwait(false));

            var sw = Stopwatch.StartNew();
            for (int j = 0; j < 5; j++)
            {
                Assert.False(await Limiter.IsLimitAsync(accessKeyID).ConfigureAwait(false));
                _output.WriteLine($"index: {j}, time: {sw.Elapsed.TotalMilliseconds}");
            }

            sw.Stop();
            var elapse = sw.Elapsed.TotalMilliseconds;

            _output.WriteLine($"elapse: {elapse}");
            Assert.True(elapse > 980);
        }
    }
}
