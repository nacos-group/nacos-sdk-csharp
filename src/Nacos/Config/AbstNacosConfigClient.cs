﻿namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Nacos.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// abstract Config Client
    /// 1. for normal usage, using HttpClientFactory
    /// 2. for ms config integrate, using HttpClient
    /// </summary>
    public abstract class AbstNacosConfigClient : INacosConfigClient
    {
        protected ILogger _logger;
        protected NacosOptions _options;
        protected List<Listener> listeners;
        protected bool isHealthServer = true;

        public string Name => "http";

        public abstract Config.Http.IHttpAgent GetAgent();

        public abstract ILocalConfigInfoProcessor GetProcessor();

        public async Task<string> GetConfigAsync(GetConfigRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            // read from local cache at first
            var config = await GetProcessor().GetFailoverAsync(GetAgent().GetName(), request.DataId, request.Group, request.Tenant).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(config))
            {
                _logger?.LogInformation($"[get-config] get failover ok, envname={GetAgent().GetName()}, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, config ={config}");
                return config;
            }

            try
            {
                config = await DoGetConfigAsync(request).ConfigureAwait(false);
            }
            catch (NacosException e) when (e.ErrorCode == NacosException.NO_RIGHT)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"[get-config] get from server error, envname={GetAgent().GetName()}, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, msg={ex.Message}");
            }

            if (!string.IsNullOrWhiteSpace(config))
            {
                _logger?.LogInformation($"[get-config] content from server {config}, envname={GetAgent().GetName()}, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}");
                await GetProcessor().SaveSnapshotAsync(GetAgent().GetName(), request.DataId, request.Group, request.Tenant, config).ConfigureAwait(false);
                return config;
            }

            config = await GetProcessor().GetSnapshotAync(GetAgent().GetName(), request.DataId, request.Group, request.Tenant).ConfigureAwait(false);

            return config;
        }

        private async Task<string> DoGetConfigAsync(GetConfigRequest request)
        {
            var responseMessage = await GetAgent().GetAsync(RequestPathValue.CONFIGS, null, request.ToDict()).ConfigureAwait(false);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return result;
                case System.Net.HttpStatusCode.NotFound:
                    await GetProcessor().SaveSnapshotAsync(GetAgent().GetName(), request.DataId, request.Group, request.Tenant, null).ConfigureAwait(false);
                    return null;
                case System.Net.HttpStatusCode.Forbidden:
                    throw new NacosException(NacosException.NO_RIGHT, $"Insufficient privilege.");
                default:
                    throw new NacosException((int)responseMessage.StatusCode, responseMessage.StatusCode.ToString());
            }
        }

        public async Task<bool> PublishConfigAsync(PublishConfigRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var responseMessage = await GetAgent().PostAsync(RequestPathValue.CONFIGS, null, request.ToDict()).ConfigureAwait(false);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    _logger?.LogInformation($"[publish-single] ok, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, config={request.Content}");
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return result.Equals("true", StringComparison.OrdinalIgnoreCase);
                case System.Net.HttpStatusCode.Forbidden:
                    _logger?.LogWarning($"[publish-single] error, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, code={(int)responseMessage.StatusCode} msg={responseMessage.StatusCode.ToString()}");
                    throw new NacosException(NacosException.NO_RIGHT, $"Insufficient privilege.");
                default:
                    _logger?.LogWarning($"[publish-single] error, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, code={(int)responseMessage.StatusCode} msg={responseMessage.StatusCode.ToString()}");
                    return false;
            }
        }

        public async Task<bool> RemoveConfigAsync(RemoveConfigRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var responseMessage = await GetAgent().DeleteAsync(RequestPathValue.CONFIGS, null, request.ToDict()).ConfigureAwait(false);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    _logger?.LogInformation($"[remove] ok, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}");
                    var result = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return result.Equals("true", StringComparison.OrdinalIgnoreCase);
                case System.Net.HttpStatusCode.Forbidden:
                    _logger?.LogWarning($"[remove] error, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, code={(int)responseMessage.StatusCode} msg={responseMessage.StatusCode.ToString()}");
                    throw new NacosException(NacosException.NO_RIGHT, $"Insufficient privilege.");
                default:
                    _logger?.LogWarning($"[remove] error, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, code={(int)responseMessage.StatusCode} msg={responseMessage.StatusCode.ToString()}");
                    return false;
            }
        }

        public Task AddListenerAsync(AddListenerRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            if (string.IsNullOrWhiteSpace(request.Tenant)) request.Tenant = _options.Namespace;
            if (string.IsNullOrWhiteSpace(request.Group)) request.Group = ConstValue.DefaultGroup;

            request.CheckParam();

            var name = BuildName(request.Tenant, request.Group, request.DataId);

            if (listeners.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger?.LogWarning($"[add-listener] error, {name} has been added.");
                return Task.CompletedTask;
            }

            var cts = new CancellationTokenSource();

            _ = PollingAsync(request, cts.Token);

            listeners.Add(new Listener(name, cts));

            return Task.CompletedTask;
        }

        public Task RemoveListenerAsync(RemoveListenerRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            if (string.IsNullOrWhiteSpace(request.Tenant)) request.Tenant = _options.Namespace;
            if (string.IsNullOrWhiteSpace(request.Group)) request.Group = ConstValue.DefaultGroup;

            request.CheckParam();

            var name = BuildName(request.Tenant, request.Group, request.DataId);

            if (!listeners.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger?.LogWarning($"[remove-listener] error, {name} was not added.");
                return Task.CompletedTask;
            }

            var list = listeners.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

            // clean timer
            foreach (var item in list)
            {
                item.Cts.Cancel();
                item.Cts.Dispose();
                item.Cts = null;
            }

            // remove listeners
            listeners.RemoveAll(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            foreach (var cb in request.Callbacks)
            {
                try
                {
                    cb();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"[remove-listener] call back throw exception, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}");
                }
            }

            return Task.CompletedTask;
        }

        private string BuildName(string tenant, string group, string dataId)
        {
            return $"{tenant}-{group}-{dataId}";
        }

        private async Task PollingAsync(AddListenerRequest request, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // read the last config
                var lastConfig = await GetProcessor().GetSnapshotAync(GetAgent().GetName(), request.DataId, request.Group, request.Tenant).ConfigureAwait(false);
                request.Content = lastConfig;

                try
                {
                    var headers = new Dictionary<string, string>()
                    {
                        { "Long-Pulling-Timeout", (ConstValue.LongPullingTimeout * 1000).ToString() }
                    };

                    var responseMessage = await GetAgent().PostAsync(RequestPathValue.CONFIGS_LISTENER, headers, request.ToDict(), cancellationToken).ConfigureAwait(false);

                    switch (responseMessage.StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                            SetHealthServer(true);
                            var content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                            await ConfigChangeAsync(content, request).ConfigureAwait(false);
                            break;
                        case System.Net.HttpStatusCode.Forbidden:
                            SetHealthServer(false);
                            _logger?.LogWarning($"[listener] error, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, code={(int)responseMessage.StatusCode} msg={responseMessage.StatusCode.ToString()}");
                            throw new NacosException(NacosException.NO_RIGHT, $"Insufficient privilege.");
                        default:
                            SetHealthServer(false);
                            _logger?.LogWarning($"[listener] error, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}, code={(int)responseMessage.StatusCode} msg={responseMessage.StatusCode.ToString()}");
                            throw new NacosException((int)responseMessage.StatusCode, responseMessage.StatusCode.ToString());
                    }
                }
                catch (Exception ex)
                {
                    SetHealthServer(false);
                    _logger?.LogError(ex, $"[listener] error, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}");
                }
            }
        }

        private async Task ConfigChangeAsync(string content, AddListenerRequest request)
        {
            // config was changed
            if (!string.IsNullOrWhiteSpace(content))
            {
                var config = await DoGetConfigAsync(new GetConfigRequest
                {
                    DataId = request.DataId,
                    Group = request.Group,
                    Tenant = request.Tenant
                }).ConfigureAwait(false);

                // update local cache
                await GetProcessor().SaveSnapshotAsync(GetAgent().GetName(), request.DataId, request.Group, request.Tenant, config).ConfigureAwait(false);

                // callback
                foreach (var cb in request.Callbacks)
                {
                    try
                    {
                        cb(config);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"[listener] call back throw exception, dataId={request.DataId}, group={request.Group}, tenant={request.Tenant}");
                    }
                }
            }
        }

        private void SetHealthServer(bool flag)
        {
            isHealthServer = flag;
        }

        public Task<string> GetServerStatus()
        {
            return Task.FromResult(isHealthServer ? "UP" : "DOWN");
        }
    }
}
