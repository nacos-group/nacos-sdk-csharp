namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using Nacos.Microsoft.Extensions.Configuration.NacosLog;
    using Serilog;
    using Xunit;

    [Trait("Category", "all")]
    public class NacosLoggerFactoryTest
    {
        [Fact]
        public void GetInstance_Should_Return_ConsoleLoggerProvider_When_Build_Is_Null()
        {
            var factory = NacosLoggerFactory.GetInstance();

            var logger = factory.CreateLogger(nameof(NacosLoggerFactory));

            Assert.NotNull(logger);
        }

        [Fact]
        public void GetInstance_Should_Return_CusLoggingProvider_When_Build_Is_Not_Null()
        {
            var factory = NacosLoggerFactory.GetInstance(x => x.AddSerilog());

            var logger = factory.CreateLogger(nameof(NacosLoggerFactory));

            Assert.NotNull(logger);
        }
    }
}
