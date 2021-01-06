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

            Init(options);

            ServerListManager serverListManager = new ServerListManager(logger, options);

            if (ParamUtils.UseHttpSwitch())
            {
                // http
                _agent = null;
            }
            else
            {
                // rpc
                _agent = null;
            }
        }

        private void Init(IOptionsMonitor<NacosSdkOptions> options)
        {
            throw new NotImplementedException();
        }

        public Task AddListeners(string dataId, string group, List<IListener> listeners)
        {
            group = Null2defaultGroup(group);
            CacheData cache = AddCacheDataIfAbsent(dataId, group);
            foreach (var listener in listeners)
            {
                // cache.AddListener(listener);
            }

            if (!cache.IsListenSuccess)
            {
                // _agent.notifyListenConfig();
            }

            return Task.CompletedTask;
        }

        private string Null2defaultGroup(string group) => (group == null) ? Constants.DEFAULT_GROUP : group.Trim();

        public CacheData AddCacheDataIfAbsent(string dataId, string group)
        {
            CacheData cache = GetCache(dataId, group);

            if (cache != null) return cache;

            string key = GroupKey.GetKey(dataId, group);
            cache = new CacheData(_configFilterChainManager, _agent.GetName(), dataId, group);

            lock (cacheMap)
            {
                CacheData cacheFromMap = GetCache(dataId, group);

                // multiple listeners on the same dataid+group and race condition,so double check again
                // other listener thread beat me to set to cacheMap
                if (cacheFromMap != null)
                {
                    cache = cacheFromMap;

                    // reset so that server not hang this check
                    // cache.SetInitializing(true);
                }
                else
                {
                    int taskId = cacheMap.Count / 3000;
                    cache.TaskId = taskId;
                }

                var copy = new Dictionary<string, CacheData>(cacheMap);
                copy[key] = cache;
                cacheMap = copy;
            }

            _logger.LogInformation("[{0}] [subscribe] {1}", this._agent.GetName(), key);

            // MetricsMonitor.getListenConfigCountMonitor().set(cacheMap.get().size());
            return cache;
        }

        public void RemoveListener(string dataId, string group, IListener listener)
        {
            group = Null2defaultGroup(group);
            CacheData cache = GetCache(dataId, group);
            if (cache != null)
            {
                /*cache.RemoveListener(listener);
                if (cache.getListeners().isEmpty())
                {
                    _agent.RemoveCache(dataId, group);
                }*/
            }
        }

        public CacheData GetCache(string dataId, string group)
        {
            return GetCache(dataId, group, TenantUtil.GetUserTenantForAcm());
        }

        public CacheData GetCache(string dataId, string group, string tenant)
        {
            if (dataId == null || group == null) throw new ArgumentException();

            return cacheMap.TryGetValue(GroupKey.GetKeyTenant(dataId, group, tenant), out var cache) ? cache : null;
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

        private async Task CheckLocalConfig(string agentName, CacheData cacheData)
        {
            string dataId = cacheData.DataId;
            string group = cacheData.Group;
            string tenant = cacheData.Tenant;

            var path = FileLocalConfigInfoProcessor.GetFailoverFile(agentName, dataId, group, tenant);

            if (!cacheData.IsUseLocalConfig && path.Exists)
            {
                string content = await FileLocalConfigInfoProcessor.GetFailoverAsync(agentName, dataId, group, tenant);
                string md5 = HashUtil.GetMd5(content);
                cacheData.SetUseLocalConfigInfo(true);
                cacheData.SetLocalConfigInfoVersion(ObjectUtil.DateTimeToTimestamp(path.LastWriteTimeUtc));
                cacheData.SetContent(content);

                _logger?.LogWarning(
                    "[{0}] [failover-change] failover file created. dataId={1}, group={2}, tenant={3}, md5={4}, content={5}",
                    agentName, dataId, group, tenant, md5, ContentUtils.TruncateContent(content));

                return;
            }

            // If use local config info, then it doesn't notify business listener and notify after getting from server.
            if (cacheData.IsUseLocalConfig && !path.Exists)
            {
                cacheData.SetUseLocalConfigInfo(false);

                _logger?.LogWarning(
                  "[{}] [failover-change] failover file deleted. dataId={}, group={}, tenant={}",
                  agentName, dataId, group, tenant);
                return;
            }

            // When it changed.
            if (cacheData.IsUseLocalConfig
                && path.Exists
                && cacheData.GetLocalConfigInfoVersion() != ObjectUtil.DateTimeToTimestamp(path.LastWriteTimeUtc))
            {
                string content = await FileLocalConfigInfoProcessor.GetFailoverAsync(agentName, dataId, group, tenant);
                string md5 = HashUtil.GetMd5(content);
                cacheData.SetUseLocalConfigInfo(true);
                cacheData.SetLocalConfigInfoVersion(ObjectUtil.DateTimeToTimestamp(path.LastWriteTimeUtc));
                cacheData.SetContent(content);

                _logger?.LogWarning(
                   "[{0}] [failover-change] failover file created. dataId={1}, group={2}, tenant={3}, md5={4}, content={5}",
                   agentName, dataId, group, tenant, md5, ContentUtils.TruncateContent(content));
            }
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
