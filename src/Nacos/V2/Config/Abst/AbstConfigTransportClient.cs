namespace Nacos.V2.Config.Abst
{
    using Nacos.V2.Security;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class AbstConfigTransportClient : IConfigTransportClient
    {
        protected NacosSdkOptions _options;
        protected IServerListManager _serverListManager;
        protected ISecurityProxy _securityProxy;

        public string GetName() => GetNameInner();

        public string GetNamespace() => GetNamespaceInner();

        public string GetTenant() => GetTenantInner();

        public Task<bool> PublishConfigAsync(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content)
            => PublishConfig(dataId, group, tenant, appName, tag, betaIps, content);

        public Task<List<string>> QueryConfigAsync(string dataId, string group, string tenat, long readTimeous, bool notify)
            => QueryConfig(dataId, group, tenat, readTimeous, notify);

        public Task<bool> RemoveConfigAsync(string dataId, string group, string tenat, string tag)
            => RemoveConfig(dataId, group, tenat, tag);

        public void Start() => StartInner();

        protected abstract string GetNameInner();

        protected abstract string GetNamespaceInner();

        protected abstract string GetTenantInner();

        protected abstract Task<bool> PublishConfig(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content);

        protected abstract Task<bool> RemoveConfig(string dataId, string group, string tenat, string tag);

        protected abstract Task<List<string>> QueryConfig(string dataId, string group, string tenat, long readTimeous, bool notify);

        protected abstract Task RemoveCache(string dataId, string group);

        protected abstract void StartInner();

        public Task RemoveCacheAsync(string dataId, string group) => RemoveCache(dataId, group);

        protected abstract Task ExecuteConfigListen();

        public Task ExecuteConfigListenAsync() => ExecuteConfigListen();
    }
}
