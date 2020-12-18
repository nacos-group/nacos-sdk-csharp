namespace Nacos.Config.Abst
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class AbstConfigTransportClient : IConfigTransportClient
    {
        protected NacosOptions _options;
        protected IServerListManager _serverListManager;
        protected Nacos.Security.V2.ISecurityProxy _securityProxy;

        public string GetName() => GetNameInner();

        public string GetNamespace() => GetNamespaceInner();

        public string GetTenant() => GetTenantInner();

        public Task<bool> PublishConfigAsync(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content)
            => PublishConfigInner(dataId, group, tenant, appName, tag, betaIps, content);

        public Task<List<string>> QueryConfigAsync(string dataId, string group, string tenat, long readTimeous, bool notify)
            => QueryConfigInner(dataId, group, tenat, readTimeous, notify);

        public Task<bool> RemoveConfigAsync(string dataId, string group, string tenat, string tag)
            => RemoveConfigInner(dataId, group, tenat, tag);

        public void Start() => StartInner();

        protected abstract string GetNameInner();

        protected abstract string GetNamespaceInner();

        protected abstract string GetTenantInner();

        protected abstract Task<bool> PublishConfigInner(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content);

        protected abstract Task<bool> RemoveConfigInner(string dataId, string group, string tenat, string tag);

        protected abstract Task<List<string>> QueryConfigInner(string dataId, string group, string tenat, long readTimeous, bool notify);

        protected abstract void StartInner();
    }
}
