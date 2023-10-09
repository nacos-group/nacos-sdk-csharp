namespace Nacos.Config.Abst
{
    using Nacos.Config.FilterImpl;
    using Nacos.Config.Impl;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IConfigTransportClient
    {
        /// <summary>
        /// get name
        /// </summary>
        string GetName();

        /// <summary>
        /// get namespace
        /// </summary>
        /// <returns>Namespace</returns>
        string GetNamespace();

        /// <summary>
        /// get tenant
        /// </summary>
        /// <returns>Tenant</returns>
        string GetTenant();

        bool GetIsHealthServer();

        Task<bool> PublishConfigAsync(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content, string encryptedDataKey, string casMd5, string type);

        Task<bool> RemoveConfigAsync(string dataId, string group, string tenant, string tag);

        Task<ConfigResponse> QueryConfigAsync(string dataId, string group, string tenant, long readTimeous, bool notify);

        Task RemoveCacheAsync(string dataId, string group, string tenant);

        CacheData AddOrUpdateCache(string key, CacheData value);

        bool TryGetCache(string key, out CacheData value);

        int GetCacheCount();

        Task ExecuteConfigListenAsync();

        Task NotifyListenConfigAsync();

        void Start();
    }
}
