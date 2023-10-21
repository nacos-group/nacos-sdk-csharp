namespace Nacos.Tests.Config.Impl
{
    using Moq;
    using Nacos.Config.Abst;
    using Nacos.Config.Common;
    using Nacos.Config.FilterImpl;
    using Nacos.Config.Impl;
    using System.Collections.Generic;
    using Xunit;

    public class ConfigRpcTransportClientTests
    {
        // TODO: Rewrite test using mocks
        private Mock<AbstConfigTransportClient> _mockAgent;

        private IConfigTransportClient _agent;

        public ConfigRpcTransportClientTests()
        {
            var options = new NacosSdkOptions
            {
                ServerAddresses = new List<string> { "http://localhost:8848/" },
                EndPoint = "",
                Namespace = "cs",
                UserName = "nacos",
                Password = "nacos",
                ConfigUseRpc = true,
            };
            _agent = new ConfigRpcTransportClient(options);
            _mockAgent = new Mock<AbstConfigTransportClient>();
        }



        [Fact]
        public void Get_Options_Shuold_Succeed()
        {
            var n = "mock_name";
            var ns = "mock_namespace";

            _mockAgent.Setup(m => m.GetName()).Returns(n);
            _mockAgent.Setup(m => m.GetNamespace()).Returns(ns);
            _mockAgent.Setup(m => m.GetTenant()).Returns(ns);

            Assert.Equal(n, _mockAgent.Object.GetName());
            Assert.Equal(ns, _mockAgent.Object.GetNamespace());
            Assert.Equal(ns, _mockAgent.Object.GetTenant());
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

            string key = GroupKey.GetKey(dataId, group, null);
            var cache = new CacheData(new ConfigFilterChainManager(new NacosSdkOptions()), _agent.GetName(), dataId, group, null);
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
