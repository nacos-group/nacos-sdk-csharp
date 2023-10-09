namespace Nacos.Tests.Config.Impl
{
    using Nacos.Config.Abst;
    using Nacos.Config.Common;
    using Nacos.Config.FilterImpl;
    using Nacos.Config.Impl;
    using System.Collections.Generic;
    using Xunit;

    public class ConfigRpcTransportClientTests
    {
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
        }

        [Fact]
        public void Get_Options_Shuold_Succeed()
        {
            var namespaces = _agent.GetNamespace();
            Assert.Equal("cs", namespaces);
            var tenant = _agent.GetTenant();
            Assert.Equal("cs", tenant);
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
    }
}
