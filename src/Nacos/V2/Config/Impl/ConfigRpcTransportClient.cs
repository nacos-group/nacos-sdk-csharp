namespace Nacos.V2.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Common;
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Remote;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConfigRpcTransportClient : AbstConfigTransportClient
    {
        private static readonly string RPC_AGENT_NAME = "config_rpc_client";

        private readonly BlockingCollection<object> _listenExecutebell = new BlockingCollection<object>(boundedCapacity: 1);

        private object _bellItem = new object();

        private ILogger _logger;

        private ConcurrentDictionary<string, CacheData> _cacheMap;
        private string uuid = System.Guid.NewGuid().ToString();

        private Timer _loginTimer;

        private long _securityInfoRefreshIntervalMills = 5000;

        public ConfigRpcTransportClient(
            ILogger logger,
            NacosSdkOptions options,
            ServerListManager serverListManager,
            ConcurrentDictionary<string, CacheData> cacheMap)
        {
            this._logger = logger;
            this._options = options;
            this._serverListManager = serverListManager;
            this._accessKey = _options.AccessKey;
            this._secretKey = _options.SecretKey;
            this._cacheMap = cacheMap;
            this._securityProxy = new Security.SecurityProxy(options, logger);

            StartInner();
        }

        protected override string GetNameInner() => RPC_AGENT_NAME;

        protected override string GetNamespaceInner() => _options.Namespace;

        protected override string GetTenantInner() => _options.Namespace;

        protected override async Task<bool> PublishConfig(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content)
        {
            try
            {
                var request = new ConfigPublishRequest(dataId, group, tenant, content);
                request.PutAdditonalParam("tag", tag);
                request.PutAdditonalParam("appName", appName);
                request.PutAdditonalParam("betaIps", betaIps);

                var response = await RequestProxy(GetOneRunningClient(), request);

                return response.IsSuccess();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[{0}] [publish-single] error, dataId={1}, group={2}, tenant={3}, code={4}", this.GetName(), dataId, group, tenant, "unkonw");
                return false;
            }
        }

        protected override async Task<List<string>> QueryConfig(string dataId, string group, string tenant, long readTimeous, bool notify)
        {
            try
            {
                var request = new ConfigQueryRequest(dataId, group, tenant);
                request.PutHeader("notify", notify.ToString());

                var response = (ConfigQueryResponse)await RequestProxy(GetOneRunningClient(), request);

                string[] ct = new string[2];

                if (response.IsSuccess())
                {
                    await FileLocalConfigInfoProcessor.SaveSnapshotAsync(this.GetName(), dataId, group, tenant, response.Content);

                    ct[0] = response.Content;
                    ct[1] = response.ContentType.IsNotNullOrWhiteSpace() ? response.ContentType : "text";

                    return ct.ToList();
                }
                else if (response.ErrorCode.Equals(ConfigQueryResponse.CONFIG_NOT_FOUND))
                {
                    await FileLocalConfigInfoProcessor.SaveSnapshotAsync(this.GetName(), dataId, group, tenant, null);
                    return ct.ToList();
                }
                else if (response.ErrorCode.Equals(ConfigQueryResponse.CONFIG_QUERY_CONFLICT))
                {
                    _logger?.LogError(
                        "[{0}] [sub-server-error] get server config being modified concurrently, dataId={1}, group={2}, tenant={3}",
                        GetName(), dataId, group, tenant);
                    throw new NacosException(NacosException.CONFLICT, $"data being modified, dataId={dataId},group={group},tenant={tenant}");
                }
                else
                {
                    _logger?.LogError(
                       "[{0}] [sub-server-error]  dataId={1}, group={2}, tenant={3}, code={4}",
                       GetName(), dataId, group, tenant, response.ToJsonString());
                    throw new NacosException(response.ErrorCode, $"http error, code={response.ErrorCode}, dataId={dataId},group={group},tenant={tenant}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[{0}] [sub-server-error] dataId={1}, group={2}, tenant={3}, code={4} ", GetName(), dataId, group, tenant, ex.Message);
                throw;
            }
        }

        protected override async Task<bool> RemoveConfig(string dataId, string group, string tenant, string tag)
        {
            try
            {
                var request = new ConfigRemoveRequest(dataId, group, tenant, tag);

                var response = await RequestProxy(GetOneRunningClient(), request);

                return response.IsSuccess();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[{0}] [remove-single] error, dataId={1}, group={2}, tenant={3}, code={4}", this.GetName(), dataId, group, tenant, "unkonw");
                return false;
            }
        }

        private void InitSecurityProxy()
        {
            _loginTimer = new Timer(
                async x =>
                {
                    await _securityProxy.LoginAsync(_serverListManager.GetServerUrls());
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_securityInfoRefreshIntervalMills));

            // init should wait the result.
            _securityProxy.LoginAsync(_serverListManager.GetServerUrls()).Wait();
        }

        protected override void StartInner()
        {
            InitSecurityProxy();

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        // block 5000ms
                        if (_listenExecutebell.TryTake(out _, 5000))
                        {
                            await ExecuteConfigListen();
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "[ rpc listen execute ] [rpc listen] exception");
                    }
                }
            });
        }

        private RpcClient EnsureRpcClient(string taskId)
        {
            Dictionary<string, string> labels = GetLabels();
            Dictionary<string, string> newlabels = new Dictionary<string, string>(labels);
            newlabels["taskId"] = taskId;

            RpcClient rpcClient = RpcClientFactory
                    .CreateClient("config-" + taskId + "-" + uuid, RemoteConnectionType.GRPC, newlabels);

            if (rpcClient.IsWaitInited())
            {
                InitHandlerRpcClient(rpcClient);
                rpcClient.SetTenant(GetTenant());
                rpcClient.SetClientAbilities(InitAbilities());
                rpcClient.Start();
            }

            return rpcClient;
        }

        private ClientAbilities InitAbilities()
        {
            ClientAbilities clientAbilities = new ClientAbilities();
            clientAbilities.RemoteAbility.SupportRemoteConnection = true;
            clientAbilities.ConfigAbility.SupportRemoteMetrics = false;
            return clientAbilities;
        }

        private void InitHandlerRpcClient(RpcClient rpcClientInner)
        {
            rpcClientInner.RegisterServerPushResponseHandler(new ConfigRpcServerRequestHandler(_cacheMap, NotifyListenConfig));
            rpcClientInner.RegisterConnectionListener(new ConfigRpcConnectionEventListener(_logger, rpcClientInner, _cacheMap, _listenExecutebell));

            rpcClientInner.Init(new ConfigRpcServerListFactory(_serverListManager));
        }

        private Dictionary<string, string> GetLabels()
        {
            var labels = new Dictionary<string, string>(6)
            {
                { RemoteConstants.LABEL_SOURCE, RemoteConstants.LABEL_SOURCE_SDK },
                { RemoteConstants.LABEL_MODULE, RemoteConstants.LABEL_MODULE_CONFIG },
                { Constants.APPNAME, AppDomain.CurrentDomain.FriendlyName },
                { Constants.VIPSERVER_TAG, "" },
                { Constants.AMORY_TAG, "" },
                { Constants.LOCATION_TAG, "" },
            };
            return labels;
        }

        private async Task<CommonResponse> RequestProxy(RpcClient rpcClientInner, CommonRequest request)
        {
            BuildRequestHeader(request);

            var timeOut = _options.DefaultTimeOut > 0 ? _options.DefaultTimeOut : 3000;

            // TODO: 1. limiter
            return await rpcClientInner.Request(request, timeOut);
        }

        private void BuildRequestHeader(CommonRequest request)
        {
            Dictionary<string, string> securityHeaders = GetSecurityHeaders();
            request.PutAllHeader(securityHeaders);

            Dictionary<string, string> spasHeaders = GetSpasHeaders();
            request.PutAllHeader(spasHeaders);

            Dictionary<string, string> commonHeaders = GetCommonHeader();
            request.PutAllHeader(commonHeaders);

            Dictionary<string, string> signHeaders = GetSignHeaders(ResourceBuild(request), _secretKey);
            request.PutAllHeader(signHeaders);
        }

        private string ResourceBuild(CommonRequest request)
        {
            if (request is ConfigQueryRequest cqReq)
                return GetResource(cqReq.Tenant, cqReq.Group);

            if (request is ConfigPublishRequest cpReq)
                return GetResource(cpReq.Tenant, cpReq.Group);

            if (request is ConfigRemoveRequest crReq)
                return GetResource(crReq.Tenant, crReq.Group);

            return string.Empty;
        }

        private string GetResource(string tenant, string group)
        {
            if (!string.IsNullOrWhiteSpace(tenant) && !string.IsNullOrWhiteSpace(group)) return tenant + "+" + group;

            if (!string.IsNullOrWhiteSpace(group)) return group;

            if (!string.IsNullOrWhiteSpace(tenant)) return tenant;

            return string.Empty;
        }

        private RpcClient GetOneRunningClient() => EnsureRpcClient("0");

        protected override Task RemoveCache(string dataId, string group)
        {
            var groupKey = GroupKey.GetKey(dataId, group);

            _cacheMap.TryRemove(groupKey, out _);

            _logger?.LogInformation("[{0}] [unsubscribe] {1}", GetNameInner(), groupKey);

            return Task.CompletedTask;
        }

        protected async override Task ExecuteConfigListen()
        {
            var listenCachesMap = new Dictionary<string, List<CacheData>>();
            var removeListenCachesMap = new Dictionary<string, List<CacheData>>();

            // TODO: should update logic here.....
            foreach (var item in _cacheMap.Values)
            {
                if (item.GetListeners() != null && item.GetListeners().Any() && !item.IsListenSuccess)
                {
                    if (!item.IsUseLocalConfig)
                    {
                        if (!listenCachesMap.TryGetValue(item.TaskId.ToString(), out var list))
                        {
                            list = new List<CacheData>();
                            listenCachesMap[item.TaskId.ToString()] = list;
                        }

                        list.Add(item);
                    }
                }
                else if ((item.GetListeners() == null || !item.GetListeners().Any()) && item.IsListenSuccess)
                {
                    if (!item.IsUseLocalConfig)
                    {
                        if (!removeListenCachesMap.TryGetValue(item.TaskId.ToString(), out var list))
                        {
                            list = new List<CacheData>();
                            removeListenCachesMap[item.TaskId.ToString()] = list;
                        }

                        list.Add(item);
                    }
                }
            }

            if (listenCachesMap != null && listenCachesMap.Any())
            {
                foreach (var task in listenCachesMap)
                {
                    var taskId = task.Key;
                    var listenCaches = task.Value;

                    var request = new ConfigBatchListenRequest() { Listen = true };

                    foreach (var item in listenCaches)
                        request.AddConfigListenContext(item.Tenant, item.Group, item.DataId, item.Md5);

                    if (request.ConfigListenContexts != null && request.ConfigListenContexts.Any())
                    {
                        try
                        {
                            var rpcClient = EnsureRpcClient(taskId);

                            var configChangeBatchListenResponse = (ConfigChangeBatchListenResponse)(await RequestProxy(rpcClient, request));

                            if (configChangeBatchListenResponse != null && configChangeBatchListenResponse.IsSuccess())
                            {
                                var changeKeys = new HashSet<string>();

                                if (configChangeBatchListenResponse.ChangedConfigs != null && configChangeBatchListenResponse.ChangedConfigs.Any())
                                {
                                    foreach (var item in configChangeBatchListenResponse.ChangedConfigs)
                                    {
                                        var changeKey = GroupKey.GetKeyTenant(item.DataId, item.Group, item.Tenant);

                                        changeKeys.Add(changeKey);

                                        await RefreshContentAndCheck(changeKey, true);
                                    }
                                }

                                foreach (var item in listenCaches)
                                {
                                    if (!changeKeys.Contains(GroupKey.GetKeyTenant(item.DataId, item.Group, item.Tenant)))
                                    {
                                        item.IsListenSuccess = true;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "async listen config change error ");
                        }
                    }
                }
            }

            if (removeListenCachesMap != null && removeListenCachesMap.Any())
            {
                foreach (var task in removeListenCachesMap)
                {
                    var taskId = task.Key;
                    var removeListenCaches = task.Value;

                    var request = new ConfigBatchListenRequest { Listen = false };

                    foreach (var item in removeListenCaches)
                        request.AddConfigListenContext(item.Tenant, item.Group, item.DataId, item.Md5);

                    if (request.ConfigListenContexts != null && request.ConfigListenContexts.Any())
                    {
                        try
                        {
                            RpcClient rpcClient = EnsureRpcClient(taskId);
                            var response = await RequestProxy(rpcClient, request);

                            if (response != null && response.IsSuccess())
                            {
                                foreach (var item in removeListenCaches) RemoveCache(item.DataId, item.Group, item.Tenant);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "async remove listen config change error ");
                        }
                    }
                }
            }
        }

        private async Task RefreshContentAndCheck(string groupKey, bool notify)
        {
            if (_cacheMap != null && _cacheMap.ContainsKey(groupKey))
            {
                _cacheMap.TryGetValue(groupKey, out var cache);
                await RefreshContentAndCheck(cache, notify);
            }
        }

        private async Task RefreshContentAndCheck(CacheData cacheData, bool notify)
        {
            try
            {
                var ct = await GetServerConfig(cacheData.DataId, cacheData.Group, cacheData.Tenant, 3000L, notify);
                cacheData.SetContent(ct[0]);
                if (ct.Count > 1 && ct[1] != null) cacheData.Type = ct[1];

                cacheData.CheckListenerMd5();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "refresh content and check md5 fail ,dataid={0},group={1},tenant={2} ", cacheData.DataId, cacheData.Group, cacheData.Tenant);
            }
        }

        public async Task<List<string>> GetServerConfig(string dataId, string group, string tenant, long readTimeout, bool notify)
        {
            if (group.IsNullOrWhiteSpace()) group = Constants.DEFAULT_GROUP;

            return await QueryConfig(dataId, group, tenant, readTimeout, notify);
        }

        private void RemoveCache(string dataId, string group, string tenant)
        {
            var groupKey = GroupKey.GetKeyTenant(dataId, group, tenant);

            _cacheMap.TryRemove(groupKey, out _);

            _logger?.LogInformation("[{0}] [unsubscribe] {1}", GetNameInner(), groupKey);
        }

        protected override Task NotifyListenConfig()
        {
            _listenExecutebell.Add(_bellItem);
            return Task.CompletedTask;
        }
    }
}
