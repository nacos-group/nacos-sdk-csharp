namespace Nacos.Tests.Config.Impl
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Moq;
    using Nacos.Config.Abst;
    using Nacos.Config.Common;
    using Nacos.Config.FilterImpl;
    using Nacos.Config.Impl;
    using Nacos.Remote;
    using Nacos.Security;
    using System.Collections.Generic;
    using Xunit;

    public class ConfigRpcTransportClientTests
    {
        private Mock<IOptions<NacosSdkOptions>> _mockOption = new Mock<IOptions<NacosSdkOptions>>();
        private Mock<IServerListFactory> _mockServerListFactory = new Mock<IServerListFactory>();
        private Mock<ISecurityProxy> _mockSecurityProxy = new Mock<ISecurityProxy>();
        private IConfigTransportClient _agent;

        public ConfigRpcTransportClientTests()
        {
            _mockOption.Setup(o => o.Value).Returns(new NacosSdkOptions
            {
                ServerAddresses = new List<string> { "http://localhost:8848/" },
                EndPoint = "",
                Namespace = "cs",
                UserName = "nacos",
                Password = "nacos",
                ConfigUseRpc = true,
            });

            var services = new ServiceCollection();
            services.AddSingleton(_mockOption.Object);
            services.AddSingleton(_mockServerListFactory.Object);
            services.AddSingleton(_mockSecurityProxy.Object);

            services.AddSingleton<IConfigTransportClient, ConfigRpcTransportClient>();
            var provider = services.BuildServiceProvider();
            _agent = provider.GetRequiredService<IConfigTransportClient>();
        }



        [Fact]
        public void Get_Options_Shuold_Succeed()
        {
            Assert.Equal("config_rpc_client", _agent.GetName());
            Assert.Equal("cs", _agent.GetNamespace());
            Assert.Equal("cs", _agent.GetTenant());
        }

        [Fact]
        public void Add_Or_Update_Cache_Shuold_Succeed()
        {
            var dataId = "t";
            var group = "g";
            var tenant = "te";

            string key = GroupKey.GetKey(dataId, group, tenant);
            var cache = new CacheData(new ConfigFilterChainManager(new NacosSdkOptions()), _agent.GetName(), dataId, group, tenant);
            _agent.AddOrUpdateCache(key, cache);
            var isGet = _agent.TryGetCache(key, out var currentCache);
            Assert.True(isGet);
            Assert.Equal(dataId, currentCache.DataId);

            cache = new CacheData(new ConfigFilterChainManager(new NacosSdkOptions()), _agent.GetName(), "t-change", group, tenant);
            _agent.AddOrUpdateCache(key, cache);
            isGet = _agent.TryGetCache(key, out currentCache);
            Assert.True(isGet);
            Assert.Equal("t-change", currentCache.DataId);
        }

        [Fact]
        public void Remove_Cache_Shuold_Succeed()
        {
            var dataId = "t";
            var group = "g";
            var tenant = "te";

            string key = GroupKey.GetKeyTenant(dataId, group, tenant);
            var cache = new CacheData(new ConfigFilterChainManager(new NacosSdkOptions()), _agent.GetName(), dataId, group, tenant);
            _agent.AddOrUpdateCache(key, cache);
            Assert.Equal(1, _agent.GetCacheCount());

            _agent.RemoveCacheAsync(dataId, group, tenant);
            Assert.Equal(0, _agent.GetCacheCount());
        }

        [Fact]
        public void Execute_Config_Listen_Shuold_Succeed()
        {
            // TODO：code test
        }
    }
}
