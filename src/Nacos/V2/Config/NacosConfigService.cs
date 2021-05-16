namespace Nacos.V2.Config
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
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

        public NacosConfigService(ILoggerFactory loggerFactory, IOptions<NacosSdkOptions> optionsAccs)
        {
            this._logger = loggerFactory.CreateLogger<NacosConfigService>();
            this._namespace = optionsAccs.Value.Namespace;
            this._worker = new ClientWorker(_logger, _configFilterChainManager, optionsAccs.Value);
        }

        public Task AddListener(string dataId, string group, IListener listener)
            => _worker.AddTenantListeners(dataId, group, new List<IListener> { listener });

        public async Task<string> GetConfig(string dataId, string group, long timeoutMs)
            => await GetConfigInner(_namespace, dataId, group, timeoutMs).ConfigureAwait(false);

        public async Task<string> GetConfigAndSignListener(string dataId, string group, long timeoutMs, IListener listener)
        {
            string content = await GetConfig(dataId, group, timeoutMs).ConfigureAwait(false);

            await _worker.AddTenantListenersWithContent(dataId, group, content, new List<IListener> { listener }).ConfigureAwait(false);
            return content;
        }

        public Task<string> GetServerStatus() => Task.FromResult(_worker.IsHealthServer() ? "UP" : "DOWN");

        public async Task<bool> PublishConfig(string dataId, string group, string content)
            => await PublishConfig(dataId, group, content, "text").ConfigureAwait(false);

        public async Task<bool> PublishConfig(string dataId, string group, string content, string type)
            => await PublishConfigInner(_namespace, dataId, group, null, null, null, content, type, null).ConfigureAwait(false);

        public async Task<bool> PublishConfigCas(string dataId, string group, string content, string casMd5)
            => await PublishConfigInner(_namespace, dataId, group, null, null, null, content, "text", casMd5).ConfigureAwait(false);

        public async Task<bool> PublishConfigCas(string dataId, string group, string content, string casMd5, string type)
            => await PublishConfigInner(_namespace, dataId, group, null, null, null, content, type, casMd5).ConfigureAwait(false);

        public async Task<bool> RemoveConfig(string dataId, string group)
            => await RemoveConfigInner(_namespace, dataId, group, null).ConfigureAwait(false);

        public Task RemoveListener(string dataId, string group, IListener listener)
            => _worker.RemoveTenantListener(dataId, group, listener);

        public Task ShutDown() => Task.CompletedTask;

        private async Task<string> GetConfigInner(string tenant, string dataId, string group, long timeoutMs)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            ParamUtils.CheckKeyParam(dataId, group);
            ConfigResponse cr = new ConfigResponse();

            cr.SetDataId(dataId);
            cr.SetTenant(tenant);
            cr.SetGroup(group);

            string encryptedDataKey = string.Empty;

            // 优先使用本地配置
            string content = await FileLocalConfigInfoProcessor.GetFailoverAsync(_worker.GetAgentName(), dataId, group, tenant).ConfigureAwait(false);
            if (content != null)
            {
                _logger?.LogWarning(
                    "[{0}] [get-config] get failover ok, dataId={1}, group={2}, tenant={3}, config={4}",
                    _worker.GetAgentName(), dataId, group, tenant, ContentUtils.TruncateContent(content));

                cr.SetContent(content);

                await FileLocalConfigInfoProcessor.GetEncryptDataKeyFailover(_worker.GetAgentName(), dataId, group, tenant).ConfigureAwait(false);
                encryptedDataKey = string.Empty;
                cr.SetEncryptedDataKey(encryptedDataKey);

                _configFilterChainManager.DoFilter(null, cr);
                content = cr.GetContent();
                return content;
            }

            try
            {
                ConfigResponse response = await _worker.GetServerConfig(dataId, group, tenant, timeoutMs, false).ConfigureAwait(false);
                cr.SetContent(response.GetContent());
                cr.SetEncryptedDataKey(response.GetEncryptedDataKey());

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

            content = await FileLocalConfigInfoProcessor.GetSnapshotAync(_worker.GetAgentName(), dataId, group, tenant).ConfigureAwait(false);
            cr.SetContent(content);

            encryptedDataKey = await FileLocalConfigInfoProcessor.GetEncryptDataKeyFailover(_worker.GetAgentName(), dataId, group, tenant).ConfigureAwait(false);
            cr.SetEncryptedDataKey(encryptedDataKey);

            _configFilterChainManager.DoFilter(null, cr);
            content = cr.GetContent();
            return content;
        }

        private async Task<bool> PublishConfigInner(string tenant, string dataId, string group, string tag, string appName, string betaIps, string content, string type, string casMd5)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            ParamUtils.CheckParam(dataId, group, content);

            ConfigRequest cr = new ConfigRequest();
            cr.SetDataId(dataId);
            cr.SetTenant(tenant);
            cr.SetGroup(group);
            cr.SetContent(content);
            cr.SetType(type);
            _configFilterChainManager.DoFilter(cr, null);
            content = cr.GetContent();
            string encryptedDataKey = (string)(cr.GetParameter("encryptedDataKey") ?? string.Empty);

            return await _worker.PublishConfig(dataId, group, tenant, appName, tag, betaIps, content, encryptedDataKey, casMd5, type).ConfigureAwait(false);
        }

        private async Task<bool> RemoveConfigInner(string tenant, string dataId, string group, string tag)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            ParamUtils.CheckKeyParam(dataId, group);
            return await _worker.RemoveConfig(dataId, group, tenant, tag).ConfigureAwait(false);
        }
    }
}
