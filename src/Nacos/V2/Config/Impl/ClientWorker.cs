namespace Nacos.V2.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Common;
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Config.FilterImpl;
    using Nacos.V2.Config.Utils;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ClientWorker : IDisposable
    {
        private readonly ILogger _logger;

        private ConfigFilterChainManager _configFilterChainManager;

        private Dictionary<string, CacheData> cacheMap = new Dictionary<string, CacheData>();

        private IConfigTransportClient _agent;

        public ClientWorker(ILogger logger, ConfigFilterChainManager configFilterChainManager, IOptionsMonitor<NacosSdkOptions> options)
        {
            _logger = logger;
            _configFilterChainManager = configFilterChainManager;

            ServerListManager serverListManager = new ServerListManager(logger, options.CurrentValue);

            _agent = options.CurrentValue.ConfigUseRpc
                ? new ConfigRpcTransportClient(logger, options.CurrentValue, serverListManager, cacheMap)
                : new ConfigHttpTransportClient(logger, options.CurrentValue, serverListManager, cacheMap);
        }

        public async Task AddTenantListeners(string dataId, string group, List<IListener> listeners)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            string tenant = _agent.GetTenant();

            CacheData cache = AddCacheDataIfAbsent(dataId, group, tenant);
            foreach (var listener in listeners)
            {
                cache.AddListener(listener);
            }

            if (!cache.IsListenSuccess)
            {
                await _agent.NotifyListenConfigAsync();
            }
        }

        internal async Task AddTenantListenersWithContent(string dataId, string group, string content, List<IListener> listeners)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            string tenant = _agent.GetTenant();
            CacheData cache = AddCacheDataIfAbsent(dataId, group, tenant);
            cache.SetContent(content);
            foreach (var listener in listeners)
            {
                cache.AddListener(listener);
            }

            // if current cache is already at listening status,do not notify.
            if (!cache.IsListenSuccess)
            {
                await _agent.NotifyListenConfigAsync();
            }
        }

        public async Task RemoveTenantListener(string dataId, string group, IListener listener)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            string tenant = _agent.GetTenant();

            CacheData cache = GetCache(dataId, group, tenant);
            if (cache != null)
            {
                cache.RemoveListener(listener);
                if ((cache.GetListeners()?.Count ?? 0) > 0)
                {
                    await _agent.RemoveCacheAsync(dataId, group);
                }
            }
        }

        public CacheData AddCacheDataIfAbsent(string dataId, string group, string tenant)
        {
            CacheData cache = GetCache(dataId, group, tenant);

            if (cache != null) return cache;

            string key = GroupKey.GetKey(dataId, group, tenant);

            lock (cacheMap)
            {
                CacheData cacheFromMap = GetCache(dataId, group, tenant);

                // multiple listeners on the same dataid+group and race condition,so double check again
                // other listener thread beat me to set to cacheMap
                if (cacheFromMap != null)
                {
                    cache = cacheFromMap;

                    // reset so that server not hang this check
                    cache.IsInitializing = true;
                }
                else
                {
                    cache = new CacheData(_configFilterChainManager, _agent.GetName(), dataId, group, tenant);

                    int taskId = cacheMap.Count / CacheData.PerTaskConfigSize;
                    cache.TaskId = taskId;
                }

                cacheMap[key] = cache;
            }

            _logger?.LogInformation("[{0}] [subscribe] {1}", this._agent.GetName(), key);

            return cache;
        }

        public CacheData GetCache(string dataId, string group)
            => GetCache(dataId, group, TenantUtil.GetUserTenantForAcm());

        public CacheData GetCache(string dataId, string group, string tenant)
        {
            if (dataId == null || group == null) throw new ArgumentException();

            return cacheMap.TryGetValue(GroupKey.GetKeyTenant(dataId, group, tenant), out var cache) ? cache : null;
        }

        internal void RemoveCache(string dataId, string group, string tenant = null)
        {
            string groupKey = tenant == null ? GroupKey.GetKey(dataId, group) : GroupKey.GetKeyTenant(dataId, group, tenant);
            lock (cacheMap)
            {
                cacheMap.Remove(groupKey);
            }

            _logger?.LogInformation("[{}] [unsubscribe] {}", this._agent.GetName(), groupKey);
        }

        public async Task<bool> RemoveConfig(string dataId, string group, string tenant, string tag)
            => await _agent.RemoveConfigAsync(dataId, group, tenant, tag);

        public async Task<bool> PublishConfig(string dataId, string group, string tenant, string appName, string tag, string betaIps,
            string content)
            => await _agent.PublishConfigAsync(dataId, group, tenant, appName, tag, betaIps, content);

        public Task<List<string>> GetServerConfig(string dataId, string group, string tenant, long readTimeout, bool notify)
        {
            if (string.IsNullOrWhiteSpace(group)) group = Constants.DEFAULT_GROUP;

            return this._agent.QueryConfigAsync(dataId, group, tenant, readTimeout, notify);
        }

        public string GetAgentName() => this._agent.GetName();

        internal bool IsHealthServer() => true;

        public void Dispose()
        {
        }
    }
}
