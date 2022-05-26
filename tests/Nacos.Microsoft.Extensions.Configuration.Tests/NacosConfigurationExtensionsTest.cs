namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using Nacos.V2;
    using Nacos.V2.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using Xunit;

    [Trait("Category", "all")]
    public class NacosConfigurationExtensionsTest
    {
        [Fact]
        public void AddNacosV2Configuration_With_Action_Should_Succeed()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddNacosV2Configuration(NCSAction);

            Assert.Single(builder.Sources);
            Assert.IsType<NacosV2ConfigurationSource>(builder.Sources[0]);
        }

        [Fact]
        public void AddNacosV2Configuration_With_Action_And_Special_Things_Should_Succeed()
        {
            var serviceProvider = BuildNacosProvider();
            var client = serviceProvider.GetService<INacosConfigService>();
            var logFactory = serviceProvider.GetService<ILoggerFactory>();

            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddNacosV2Configuration(NCSAction, client, logFactory);

            Assert.Single(builder.Sources);

            var source = builder.Sources[0];
            Assert.IsType<NacosV2ConfigurationSource>(source);

            Assert.Equal(client, ((NacosV2ConfigurationSource)source).Client);
            Assert.Equal(logFactory, ((NacosV2ConfigurationSource)source).LoggerFactory);
        }

        [Fact]
        public void AddNacosV2Configuration_With_IConfiguration_Should_Succeed()
        {
            IConfiguration configuration = BuildNacosConfigIConfiguration();

            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddNacosV2Configuration(configuration);

            Assert.Single(builder.Sources);
        }

        [Fact]
        public void AddNacosV2Configuration_With_IConfiguration_And_Special_Things_Should_Succeed()
        {
            var serviceProvider = BuildNacosProvider();
            var client = serviceProvider.GetService<INacosConfigService>();
            var logFactory = serviceProvider.GetService<ILoggerFactory>();

            IConfiguration configuration = BuildNacosConfigIConfiguration();

            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddNacosV2Configuration(configuration, client, logFactory);

            Assert.Single(builder.Sources);

            var source = builder.Sources[0];
            Assert.IsType<NacosV2ConfigurationSource>(source);

            Assert.Equal(client, ((NacosV2ConfigurationSource)source).Client);
            Assert.Equal(logFactory, ((NacosV2ConfigurationSource)source).LoggerFactory);
        }

        [Fact]
        public void AddNacosV2Configuration_With_Action_Should_Throw_Exception_When_Something_IsNull()
        {
            Action<NacosV2ConfigurationSource> action = null;

            IConfigurationBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.AddNacosV2Configuration(action));

            IConfigurationBuilder builder2 = new ConfigurationBuilder();
            Assert.Throws<ArgumentNullException>(() => builder2.AddNacosV2Configuration(action));
        }

        [Fact]
        public void AddNacosV2Configuration_With_IConfiguration_Should_Throw_Exception_When_Something_IsNull()
        {
            IConfiguration configuration = null;

            IConfigurationBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.AddNacosV2Configuration(configuration));

            IConfigurationBuilder builder2 = new ConfigurationBuilder();
            Assert.Throws<ArgumentNullException>(() => builder2.AddNacosV2Configuration(configuration));
        }

        private static readonly Action<NacosV2ConfigurationSource> NCSAction = x =>
        {
            x.ServerAddresses = new List<string> { "http://localhost:8848/" };
            x.Namespace = "cs-test";
            x.ConfigUseRpc = true;
        };

        private IConfiguration BuildNacosConfigIConfiguration(string prefix = "")
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { $"{prefix}ServerAddresses:0", "http://localhost:8848/" },
                    { $"{prefix}Namespace", "cs-test" },
                    { $"{prefix}ConfigUseRpc", "true" },
                }).Build();
        }

        private ServiceProvider BuildNacosProvider()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            IConfiguration configuration = BuildNacosConfigIConfiguration("nacos:");
            serviceCollection.AddNacosV2Config(configuration);
            serviceCollection.AddLogging(x => x.AddConsole());

            return serviceCollection.BuildServiceProvider();
        }
    }
}
