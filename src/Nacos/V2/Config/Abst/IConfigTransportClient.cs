namespace Nacos.V2.Config.Abst
{
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

        Task<bool> PublishConfigAsync(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content);

        Task<bool> RemoveConfigAsync(string dataId, string group, string tenat, string tag);

        Task<List<string>> QueryConfigAsync(string dataId, string group, string tenat, long readTimeous, bool notify);

        Task AddListenerAsync(string dataId, string group, string tenant, List<Action<string>> callBacks);

        Task RemoveListenerAsync(string dataId, string group, string tenant, Action<string> callBack);

        void Start();
    }
}
