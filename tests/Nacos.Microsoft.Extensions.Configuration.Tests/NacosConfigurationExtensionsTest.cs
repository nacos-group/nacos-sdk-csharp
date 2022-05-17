namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using Xunit;

    [Trait("Category", "all")]
    public class NacosConfigurationExtensionsTest
    {
        [Fact]
        public void AddNacosV2Configuration_With_Action_Should_Succeed()
        {
            IServiceCollection services = new ServiceCollection();

            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddNacosV2Configuration(
                x =>
                {
                    x.ServerAddresses = new List<string> { "http://localhost:8848/" };
                    x.Namespace = "cs-test";
                    x.ConfigUseRpc = true;
                }, services);

            Assert.Single(builder.Sources);
        }

        [Fact]
        public void AddNacosV2Configuration_With_IConfiguration_Should_Succeed()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ServerAddresses:0", "http://localhost:8848/" },
                    { "Namespace", "cs-test" },
                    { "ConfigUseRpc", "true" },
                }).Build();

            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddNacosV2Configuration(configuration, services);

            Assert.Single(builder.Sources);
        }

        [Fact]
        public void AddNacosV2Configuration_With_Action_Should_Throw_Exception_When_Something_IsNull()
        {
            Action<NacosV2ConfigurationSource> action = null;
            IServiceCollection services = new ServiceCollection();

            IConfigurationBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.AddNacosV2Configuration(action, null));

            IConfigurationBuilder builder2 = new ConfigurationBuilder();
            Assert.Throws<ArgumentNullException>(() => builder2.AddNacosV2Configuration(action, services));
        }

        [Fact]
        public void AddNacosV2Configuration_With_IConfiguration_Should_Throw_Exception_When_Something_IsNull()
        {
            IConfiguration configuration = null;
            IServiceCollection services = new ServiceCollection();

            IConfigurationBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.AddNacosV2Configuration(configuration, null));

            IConfigurationBuilder builder2 = new ConfigurationBuilder();
            Assert.Throws<ArgumentNullException>(() => builder2.AddNacosV2Configuration(configuration, services));
        }
    }
}
