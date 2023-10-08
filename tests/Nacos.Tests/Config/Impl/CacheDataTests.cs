namespace Nacos.Tests.Config.Impl
{
    using Nacos.Config.FilterImpl;
    using Nacos.Config.Impl;
    using Nacos.Tests.Base;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class CacheDataTests
    {
        private readonly CacheData _cacheData;

        public CacheDataTests()
        {
            var configFilterChainManager = new ConfigFilterChainManager(new NacosSdkOptions
            {
                ServerAddresses = new List<string> { "http://localhost:8848/" },
                EndPoint = "",
                Namespace = "cs",
                UserName = "nacos",
                Password = "nacos",
                ConfigUseRpc = true,
            });
            _cacheData = new CacheData(configFilterChainManager, "config_rpc_client", "test", "g");
        }

        [Fact]
        public void Add_And_Remove_Listener_Should_Succeed()
        {
            var listener = new TestConfigListen();

            _cacheData.AddListener(listener);
            var listeners = _cacheData.GetListeners();
            Assert.Equal(listener, listeners.FirstOrDefault());

            _cacheData.RemoveListener(listener);
            listeners = _cacheData.GetListeners();
            Assert.Empty(listeners);
        }

        [Fact]
        public void Set_Use_Local_Config_Info_Should_Succeed()
        {
            _cacheData.SetUseLocalConfigInfo(true);
            Assert.True(_cacheData.IsUseLocalConfig);

            _cacheData.SetUseLocalConfigInfo(false);
            Assert.False(_cacheData.IsUseLocalConfig);
            Assert.Equal(-1, _cacheData.LocalConfigLastModified);
        }

        [Fact]
        public void Check_Listener_Md5_Should_Succeed()
        {
            // TODO: code test
        }
    }
}
