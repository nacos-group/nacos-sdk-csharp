namespace Nacos.Config.Core
{
    using Nacos.Config.Cache;
    using Nacos.Config.Filter;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IClientWorker : IDisposable
    {
        Task AddTenantListeners(string dataId, string group, List<IListener> listeners);

        Task AddTenantListenersWithContent(string dataId, string group, string content, List<IListener> listeners);

        Task RemoveTenantListener(string dataId, string group, IListener listener);

        CacheData AddCacheDataIfAbsent(string dataId, string group, string tenant);

        CacheData GetCache(string dataId, string group);

        CacheData GetCache(string dataId, string group, string tenant);

        void RemoveCache(string dataId, string group, string tenant = null);

        Task<bool> RemoveConfig(string dataId, string group, string tenant, string tag);

        Task<bool> PublishConfig(string dataId, string group, string tenant, string appName, string tag, string betaIps,
            string content, string encryptedDataKey, string casMd5, string type);

        Task<ConfigResponse> GetServerConfig(string dataId, string group, string tenant, long readTimeout, bool notify);

        string GetAgentName();

        bool IsHealthServer();
    }
}
