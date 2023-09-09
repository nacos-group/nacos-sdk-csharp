﻿namespace Nacos.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Auth;
    using Nacos.Common;
    using Nacos.Config;
    using Nacos.Config.Abst;
    using Nacos.Config.Common;
    using Nacos.Config.FilterImpl;
    using Nacos.Config.Utils;
    using Nacos.Logging;
    using Nacos.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ClientWorker : IClientWorker
    {
        private readonly ILogger _logger = NacosLogManager.CreateLogger<ClientWorker>();
        private ConcurrentDictionary<string, CacheData> _cacheMap = new();
        private readonly IConfigFilterChain _configFilterChainManager;

        private IConfigTransportClient _agent;

        public ClientWorker(
            IConfigFilterChain configFilterChainManager,
            IConfigTransportClient agent)
        {
            _configFilterChainManager = configFilterChainManager;
            _agent = agent;
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
                await _agent.NotifyListenConfigAsync().ConfigureAwait(false);
            }
        }

        public async Task AddTenantListenersWithContent(string dataId, string group, string content, List<IListener> listeners)
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
                await _agent.NotifyListenConfigAsync().ConfigureAwait(false);
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
                    await _agent.RemoveCacheAsync(dataId, group).ConfigureAwait(false);
                }
            }
        }

        public CacheData AddCacheDataIfAbsent(string dataId, string group, string tenant)
        {
            CacheData cache = GetCache(dataId, group, tenant);

            if (cache != null) return cache;

            string key = GroupKey.GetKey(dataId, group, tenant);
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

                int taskId = _cacheMap.Count / CacheData.PerTaskConfigSize;
                cache.TaskId = taskId;
            }

            _cacheMap.AddOrUpdate(key, cache, (x, y) => cache);

            _logger?.LogInformation("[{0}] [subscribe] {1}", _agent.GetName(), key);

            return cache;
        }

        public CacheData GetCache(string dataId, string group)
            => GetCache(dataId, group, TenantUtil.GetUserTenantForAcm());

        public CacheData GetCache(string dataId, string group, string tenant)
        {
            if (dataId == null || group == null) throw new ArgumentException();

            return _cacheMap.TryGetValue(GroupKey.GetKeyTenant(dataId, group, tenant), out var cache) ? cache : null;
        }

        public void RemoveCache(string dataId, string group, string tenant = null)
        {
            string groupKey = tenant == null ? GroupKey.GetKey(dataId, group) : GroupKey.GetKeyTenant(dataId, group, tenant);

            _cacheMap.TryRemove(groupKey, out _);

            _logger?.LogInformation("[{0}] [unsubscribe] {1}", _agent.GetName(), groupKey);
        }

        public async Task<bool> RemoveConfig(string dataId, string group, string tenant, string tag)
            => await _agent.RemoveConfigAsync(dataId, group, tenant, tag).ConfigureAwait(false);

        public async Task<bool> PublishConfig(string dataId, string group, string tenant, string appName, string tag, string betaIps,
            string content, string encryptedDataKey, string casMd5, string type)
            => await _agent.PublishConfigAsync(dataId, group, tenant, appName, tag, betaIps, content, encryptedDataKey, casMd5, type).ConfigureAwait(false);

        public Task<ConfigResponse> GetServerConfig(string dataId, string group, string tenant, long readTimeout, bool notify)
        {
            if (group.IsNullOrWhiteSpace()) group = Constants.DEFAULT_GROUP;

            return _agent.QueryConfigAsync(dataId, group, tenant, readTimeout, notify);
        }

        public string GetAgentName() => _agent.GetName();

        public bool IsHealthServer() => _agent.GetIsHealthServer();

        public void Dispose()
        {
        }
    }
}
