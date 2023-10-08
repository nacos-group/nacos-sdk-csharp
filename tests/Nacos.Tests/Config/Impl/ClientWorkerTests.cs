namespace Nacos.Tests.Config.Impl
{
    using Nacos.Config.Abst;
    using Nacos.Config.FilterImpl;
    using Nacos.Config.Impl;
    using Nacos.Tests.Base;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Xunit;

    public class ClientWorkerTests
    {
        private const string DATA_ID = "test";
        private const string GROUP = "g";
        private const string TENANT = "cs";
        private readonly ConfigFilterChainManager _configFilterChainManager;
        private readonly IClientWorker _worker;

        public ClientWorkerTests()
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
            _configFilterChainManager = new ConfigFilterChainManager(options);
            var agent = new ConfigRpcTransportClient(options);
            _worker = new ClientWorker(_configFilterChainManager, agent);
        }

        [Fact]
        public async Task Publish_Config_Should_Succeed()
        {
            var res = await PublishConfig("publish content").ConfigureAwait(false);
            Assert.True(res);
        }

        [Fact]
        public async Task Add_And_Remove_Tenant_Listeners_Should_Succeed()
        {
            var listener = new TestConfigListen();

            // Add tenant listeners
            await _worker.AddTenantListeners(DATA_ID, GROUP, new List<Nacos.Config.IListener> { listener }).ConfigureAwait(false);
            var cacheData = _worker.GetCache(DATA_ID, GROUP, TENANT);
            var listeners = cacheData.GetListeners();
            Assert.Equal(listener, listeners.FirstOrDefault());

            // Remove tenant listeners
            await _worker.RemoveTenantListener(DATA_ID, GROUP, listener).ConfigureAwait(false);
            listeners = cacheData.GetListeners();
            Assert.Empty(listeners);
        }

        [Fact]
        public async Task Add_Tenant_Listeners_With_Content_Should_Succeed()
        {
            var content = "content";
            var listener = new TestConfigListen();

            await _worker.AddTenantListenersWithContent(DATA_ID, GROUP, content, new List<Nacos.Config.IListener> { listener }).ConfigureAwait(false);
            var cacheData = _worker.GetCache(DATA_ID, GROUP, TENANT);
            var listeners = cacheData.GetListeners();
            Assert.Equal(listener, listeners.FirstOrDefault());
            Assert.Equal(content, cacheData.Content);
        }

        [Fact]
        public async Task Get_Server_Config_Should_Succeed()
        {
            var configContent = "test get server config content";
            await PublishConfig(configContent).ConfigureAwait(false);

            ConfigResponse cr = new();
            ConfigResponse response = await _worker.GetServerConfig(DATA_ID, GROUP, TENANT, 3000, false).ConfigureAwait(false);

            cr.SetContent(response.GetContent());
            cr.SetEncryptedDataKey(response.GetEncryptedDataKey());

            _configFilterChainManager.DoFilter(null, cr);
            var content = cr.GetContent();
            Assert.Equal(configContent, content);
        }


        [Fact]
        public async Task Remove_Config_Should_Succeed()
        {
            var res = await _worker.RemoveConfig(DATA_ID, GROUP, TENANT, null).ConfigureAwait(false);
            Assert.True(res);
        }

        private async Task<bool> PublishConfig(string content)
        {
            var type = "text";
            ConfigRequest cr = new();
            cr.SetDataId(DATA_ID);
            cr.SetTenant(TENANT);
            cr.SetGroup(GROUP);
            cr.SetContent(content);
            cr.SetType(type);
            _configFilterChainManager.DoFilter(cr, null);
            content = cr.GetContent();
            string encryptedDataKey = cr.GetEncryptedDataKey();

            return await _worker.PublishConfig(DATA_ID, GROUP, TENANT, null, null, null, content, encryptedDataKey, null, type).ConfigureAwait(false);
        }
    }
}
