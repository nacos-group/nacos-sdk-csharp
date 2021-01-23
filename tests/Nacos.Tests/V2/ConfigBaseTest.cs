namespace Nacos.Tests.V2
{
    using Nacos.V2;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class ConfigBaseTest
    {
        protected INacosConfigService _configSvc;

        protected ITestOutputHelper _output;

        [Fact]
        protected virtual async Task PublishConfig_Should_Succeed()
        {
            var dataId = Guid.NewGuid().ToString();
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;
            var val = "test-value";

            var pubFlag = await _configSvc.PublishConfig(dataId, group, val);
            _output.WriteLine($"PublishConfig_Should_Succeed, PublishConfig return {pubFlag}");
            Assert.True(pubFlag);
        }

        [Fact]
        protected virtual async Task GetConfig_Should_Succeed()
        {
            var dataId = Guid.NewGuid().ToString();
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;
            var val = "test-value";

            var pubFlag = await _configSvc.PublishConfig(dataId, group, val);
            _output.WriteLine($"GetConfig_Should_Succeed, PublishConfig return {pubFlag}");
            Assert.True(pubFlag);

            await Task.Delay(100);

            var config = await _configSvc.GetConfig(dataId, group, 3000L);
            _output.WriteLine($"GetConfig_Should_Succeed, GetConfig return {pubFlag}");
            Assert.Equal(val, config);
        }

        [Fact]
        protected virtual async Task DeleteConfig_Should_Succeed()
        {
            var dataId = Guid.NewGuid().ToString();
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;
            var val = "test-value";

            var pubFlag = await _configSvc.PublishConfig(dataId, group, val);
            _output.WriteLine($"DeleteConfig_Should_Succeed, PublishConfig return {pubFlag}");
            Assert.True(pubFlag);

            await Task.Delay(100);

            var config1 = await _configSvc.GetConfig(dataId, group, 3000L);
            _output.WriteLine($"DeleteConfig_Should_Succeed, GetConfig1 return {config1}");
            Assert.Equal(val, config1);

            var remFlag = await _configSvc.RemoveConfig(dataId, group);
            _output.WriteLine($"DeleteConfig_Should_Succeed, RemoveConfig return {remFlag}");
            Assert.True(remFlag);

            await Task.Delay(100);

            var config2 = await _configSvc.GetConfig(dataId, group, 3000L);
            _output.WriteLine($"DeleteConfig_Should_Succeed, GetConfig2 return {config2}");
            Assert.Null(config2);
        }

        [Fact]
        protected virtual async Task ListenConfig_Should_Succeed()
        {
            var dataId = Guid.NewGuid().ToString();
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;
            var val = "test-value";

            var pubFlag = await _configSvc.PublishConfig(dataId, group, val);
            _output.WriteLine($"ListenConfig_Should_Succeed, PublishConfig1 return {pubFlag}");
            Assert.True(pubFlag);

            var listener = new TestListener();
            await _configSvc.AddListener(dataId, group, listener);

            var pubFlag2 = await _configSvc.PublishConfig(dataId, group, "123");
            _output.WriteLine($"ListenConfig_Should_Succeed, PublishConfig2 return {pubFlag}");
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
