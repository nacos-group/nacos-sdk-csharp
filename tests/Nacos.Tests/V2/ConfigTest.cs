namespace Nacos.Tests.V2
{
    using Microsoft.Extensions.DependencyInjection;
    using Nacos.V2;
    using Nacos.V2.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class ConfigTest
    {
        protected INacosConfigService _configSvc;

        public ConfigTest()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Config(x =>
            {
                x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs-test";

                // swich to use http or rpc
                x.ConfigUseRpc = true;
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _configSvc = serviceProvider.GetService<INacosConfigService>();
        }

        [Fact]
        public async Task PublishConfig_Should_Succeed()
        {
            var dataId = Guid.NewGuid().ToString();
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;
            var val = "test-value";

            var pubFlag = await _configSvc.PublishConfig(dataId, group, val);

            Assert.True(pubFlag);
        }

        [Fact]
        public async Task GetConfig_Should_Succeed()
        {
            var dataId = Guid.NewGuid().ToString();
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;
            var val = "test-value";

            var pubFlag = await _configSvc.PublishConfig(dataId, group, val);
            Assert.True(pubFlag);

            await Task.Delay(100);

            var config = await _configSvc.GetConfig(dataId, group, 3000L);
            Assert.Equal(val, config);
        }

        [Fact]
        public async Task DeleteConfig_Should_Succeed()
        {
            var dataId = Guid.NewGuid().ToString();
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;
            var val = "test-value";

            var pubFlag = await _configSvc.PublishConfig(dataId, group, val);
            Assert.True(pubFlag);

            await Task.Delay(100);

            var config1 = await _configSvc.GetConfig(dataId, group, 3000L);
            Assert.Equal(val, config1);

            var remFlag = await _configSvc.RemoveConfig(dataId, group);
            Assert.True(remFlag);

            await Task.Delay(100);

            var config2 = await _configSvc.GetConfig(dataId, group, 3000L);
            Assert.Null(config2);
        }

        [Fact]
        public async Task ListenConfig_Should_Succeed()
        {
            var dataId = Guid.NewGuid().ToString();
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;
            var val = "test-value";

            var pubFlag = await _configSvc.PublishConfig(dataId, group, val);
            Assert.True(pubFlag);

            var listener = new TestListener();
            await _configSvc.AddListener(dataId, group, listener);

            var pubFlag2 = await _configSvc.PublishConfig(dataId, group, "123");
            Assert.True(pubFlag2);
        }

        public class TestListener : Nacos.V2.IListener
        {
            public void ReceiveConfigInfo(string configInfo)
            {
                Assert.Equal("123", configInfo);
            }
        }
    }
}
