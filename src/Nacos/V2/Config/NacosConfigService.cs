namespace Nacos.V2.Config
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Common;
    using Nacos.V2.Config.FilterImpl;
    using Nacos.V2.Config.Impl;
    using Nacos.V2.Config.Utils;
    using Nacos.V2.Exceptions;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class NacosConfigService : INacosConfigService
    {
        private readonly ILogger _logger;
        private readonly ClientWorker _worker;
        private string _namespace;

        private readonly ConfigFilterChainManager _configFilterChainManager = new ConfigFilterChainManager();

        public NacosConfigService(ILoggerFactory loggerFactory, IOptionsMonitor<NacosSdkOptions> optionsAccs)
        {
            this._logger = loggerFactory.CreateLogger<NacosConfigService>();
            this._namespace = optionsAccs.CurrentValue.Namespace;
            this._worker = new ClientWorker(_logger, _configFilterChainManager, optionsAccs);
        }

        public Task AddListener(string dataId, string group, IListener listener)
            => _worker.AddTenantListeners(dataId, group, new List<IListener> { listener });

        public async Task<string> GetConfig(string dataId, string group, long timeoutMs)
            => await GetConfigInner(_namespace, dataId, group, timeoutMs);

        public async Task<string> GetConfigAndSignListener(string dataId, string group, long timeoutMs, IListener listener)
        {
            string content = await GetConfig(dataId, group, timeoutMs);

            _worker.AddTenantListenersWithContent(dataId, group, content, new List<IListener> { listener });
            return content;
        }

        public Task<string> GetServerStatus() => Task.FromResult(_worker.IsHealthServer() ? "UP" : "DOWN");

        public async Task<bool> PublishConfig(string dataId, string group, string content)
            => await PublishConfig(dataId, group, content, "text");

        public async Task<bool> PublishConfig(string dataId, string group, string content, string type)
            => await PublishConfigInner(dataId, group, content, null, null, null, content, type);

        public async Task<bool> RemoveConfig(string dataId, string group)
            => await RemoveConfigInner(_namespace, dataId, group, null);

        public Task RemoveListener(string dataId, string group, IListener listener)
            => _worker.RemoveTenantListener(dataId, group, listener);

        public Task ShutDown() => Task.CompletedTask;

        private async Task<string> GetConfigInner(string tenant, string dataId, string group, long timeoutMs)
        {
            group = Null2DefaultGroup(group);
            ParamUtils.CheckKeyParam(dataId, group);
            ConfigResponse cr = new ConfigResponse();

            cr.SetDataId(dataId);
            cr.SetTenant(tenant);
            cr.SetGroup(group);

            // 优先使用本地配置
            string content = await FileLocalConfigInfoProcessor.GetFailoverAsync(_worker.GetAgentName(), dataId, group, tenant);
            if (content != null)
            {
                _logger?.LogWarning(
                    "[{0}] [get-config] get failover ok, dataId={1}, group={2}, tenant={3}, config={4}",
                    _worker.GetAgentName(), dataId, group, tenant, ContentUtils.TruncateContent(content));

                cr.SetContent(content);
                _configFilterChainManager.DoFilter(null, cr);
                content = cr.GetContent();
                return content;
            }

            try
            {
                List<string> ct = await _worker.GetServerConfig(dataId, group, tenant, timeoutMs, false);
                cr.SetContent(ct[0]);

                _configFilterChainManager.DoFilter(null, cr);
                content = cr.GetContent();

                return content;
            }
            catch (NacosException ioe)
            {
                if (NacosException.NO_RIGHT == ioe.ErrorCode) throw;

                _logger?.LogWarning(
                  "[{0}] [get-config] get from server error, dataId={1}, group={2}, tenant={3}, msg={4}",
                  _worker.GetAgentName(), dataId, group, tenant, ioe.ErrorMsg);
            }

            _logger?.LogWarning(
                 "[{0}] [get-config] get snapshot ok, dataId={1}, group={2}, tenant={3}, config={4}",
                 _worker.GetAgentName(), dataId, group, tenant, ContentUtils.TruncateContent(content));

            content = await FileLocalConfigInfoProcessor.GetSnapshotAync(_worker.GetAgentName(), dataId, group, tenant);
            cr.SetContent(content);
            _configFilterChainManager.DoFilter(null, cr);
            content = cr.GetContent();
            return content;
        }

        private async Task<bool> PublishConfigInner(string tenant, string dataId, string group, string tag, string appName,
            string betaIps, string content, string type)
        {
            group = Null2DefaultGroup(group);
            ParamUtils.CheckParam(dataId, group, content);

            ConfigRequest cr = new ConfigRequest();
            cr.SetDataId(dataId);
            cr.SetTenant(tenant);
            cr.SetGroup(group);
            cr.SetContent(content);
            cr.SetType(type);
            _configFilterChainManager.DoFilter(cr, null);
            content = cr.GetContent();

            return await _worker.PublishConfig(dataId, group, tenant, appName, tag, betaIps, content);
        }

        private async Task<bool> RemoveConfigInner(string tenant, string dataId, string group, string tag)
        {
            group = Null2DefaultGroup(group);
            ParamUtils.CheckKeyParam(dataId, group);
            return await _worker.RemoveConfig(dataId, group, tenant, tag);
        }

        private string Null2DefaultGroup(string group) => (group == null) ? Constants.DEFAULT_GROUP : group.Trim();
    }
}
