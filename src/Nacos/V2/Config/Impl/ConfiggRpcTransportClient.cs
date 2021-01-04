namespace Nacos.V2.Config.Impl
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Remote;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConfiggRpcTransportClient : AbstConfigTransportClient
    {
        private ILogger _logger;

        private Dictionary<string, CacheData> cacheMap = new Dictionary<string, CacheData>();
        private string uuid = System.Guid.NewGuid().ToString();

        private readonly object _lock = new object();

        private Timer _configListenTimer;

        public ConfiggRpcTransportClient(
            ILoggerFactory loggerFactory,
            IOptionsMonitor<NacosSdkOptions> optionAccs)
        {
            _logger = loggerFactory.CreateLogger<GrpcConfigClient>();
            _options = optionAccs.CurrentValue;
            _serverListManager = new ServerListManager(_logger, optionAccs);
            StartInner();
        }

        protected override string GetNameInner() => "config_rpc_client";

        protected override string GetNamespaceInner()
        {
            throw new NotImplementedException();
        }

        protected override string GetTenantInner() => _options.Namespace;

        protected override async Task<bool> PublishConfigInner(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content)
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
                _logger.LogWarning(ex, "[{0}] [publish-single] error, dataId={1}, group={2}, tenant={3}, code={4}", this.GetName(), dataId, group, tenant, "unkonw");
                return false;
            }
        }

        protected override async Task<List<string>> QueryConfigInner(string dataId, string group, string tenant, long readTimeous, bool notify)
        {
            try
            {
                var request = new ConfigQueryRequest(dataId, group, tenant);
                request.PutHeader("notify", notify.ToString());

                var response = (ConfigQueryResponse)(await RequestProxy(GetOneRunningClient(), request));

                var ct = new List<string>();

                if (response.IsSuccess())
                {
                    await FileLocalConfigInfoProcessor.SaveSnapshotAsync(this.GetName(), dataId, group, tenant, response.Content);

                    ct.Add(response.Content);
                    ct.Add(string.IsNullOrWhiteSpace(response.ContentType) ? response.ContentType : "text");
                    return ct;
                }
                else if (response.ErrorCode.Equals(ConfigQueryResponse.CONFIG_NOT_FOUND))
                {
                    await FileLocalConfigInfoProcessor.SaveSnapshotAsync(this.GetName(), dataId, group, tenant, null);
                    return ct;
                }
                else if (response.ErrorCode.Equals(ConfigQueryResponse.CONFIG_QUERY_CONFLICT))
                {
                    _logger.LogError(
                        "[{0}] [sub-server-error] get server config being modified concurrently, dataId={1}, group={2}, tenant={3}",
                        GetName(), dataId, group, tenant);
                    throw new NacosException(NacosException.CONFLICT, $"data being modified, dataId={dataId},group={group},tenant={tenant}");
                }
                else
                {
                    _logger.LogError(
                       "[{0}] [sub-server-error]  dataId={1}, group={2}, tenant={3}, code={4}",
                       GetName(), dataId, group, tenant, response.ToJsonString());
                    throw new NacosException(response.ErrorCode, $"http error, code={response.ErrorCode}, dataId={dataId},group={group},tenant={tenant}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{0}] [sub-server-error] dataId={1}, group={2}, tenant={3}, code={4} ", GetName(), dataId, group, tenant, ex.Message);
                throw;
            }
        }

        protected override async Task<bool> RemoveConfigInner(string dataId, string group, string tenant, string tag)
        {
            try
            {
                var request = new ConfigRemoveRequest(dataId, group, tenant, tag);

                var response = await RequestProxy(GetOneRunningClient(), request);

                return response.IsSuccess();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[{0}] [remove-single] error, dataId={1}, group={2}, tenant={3}, code={4}", this.GetName(), dataId, group, tenant, "unkonw");
                return false;
            }
        }

        protected override void StartInner()
        {
            _configListenTimer = new Timer(
                async x =>
                {
                    await ExecuteConfigListenAsync();
                }, null, 0, 5000);
        }

        private async Task ExecuteConfigListenAsync()
        {
            var listenCachesMap = new Dictionary<string, List<CacheData>>();
            var removeListenCachesMap = new Dictionary<string, List<CacheData>>();

            foreach (var item in cacheMap.Values)
            {
                if (item.Listeners != null && item.Listeners.Any() && !item.IsListenSuccess)
                {
                    if (!listenCachesMap.TryGetValue(item.TaskId.ToString(), out var list))
                    {
                        list = new List<CacheData>();
                        listenCachesMap[item.TaskId.ToString()] = list;
                    }

                    list.Add(item);
                }
                else if ((item.Listeners == null || !item.Listeners.Any()) && item.IsListenSuccess)
                {
                    if (!removeListenCachesMap.TryGetValue(item.TaskId.ToString(), out var list))
                    {
                        list = new List<CacheData>();
                        removeListenCachesMap[item.TaskId.ToString()] = list;
                    }

                    list.Add(item);
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
                                HashSet<string> changeKeys = new HashSet<string>();

                                if (configChangeBatchListenResponse.ChangedConfigs != null && configChangeBatchListenResponse.ChangedConfigs.Any())
                                {
                                    foreach (var item in configChangeBatchListenResponse.ChangedConfigs)
                                    {
                                        var changeKey = GroupKey.GetKeyTenant(item.DataId, item.Group, item.Tenant);

                                        changeKeys.Add(changeKey);

                                        if (cacheMap.TryGetValue(changeKey, out var cached))
                                        {
                                            // query and exec
                                            var ct = await QueryConfigInner(cached.DataId, cached.Group, cached.Tenant, 3000, false);

                                            Console.WriteLine($"new config = {ct.ToJsonString()}");

                                            // check last md5
                                            if (ct.Count > 0)
                                            {
                                                cached.SetContent(ct[0]);

                                                if (cached.CheckListenerMd5()) cached.Listeners.ForEach(x => x.Invoke(ct[0]));
                                            }
                                        }
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
                            _logger.LogError(ex, "async listen config change error ");
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
                                // do remove
                                Console.WriteLine("do remove");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "async remove listen config change error ");
                        }
                    }
                }
            }
        }

        protected override Task AddListenerInner(string dataId, string group, string tenant, List<Action<string>> callBacks)
        {
            tenant = string.IsNullOrWhiteSpace(tenant) ? _options.Namespace : tenant;
            group = string.IsNullOrWhiteSpace(group) ? ConstValue.DefaultGroup : group;

            var cache = GetCache(dataId, group, tenant);
            if (cache != null) return Task.CompletedTask;

            string key = Nacos.V2.Config.GroupKey.GetKeyTenant(dataId, group, tenant);

            if (cacheMap.TryGetValue(key, out var cached)) return Task.CompletedTask;

            cache = new CacheData { DataId = dataId, Group = group, Tenant = tenant, Content = string.Empty, LastMd5 = Nacos.Utilities.HashUtil.GetMd5(string.Empty) };

            lock (_lock)
            {
                var taskId = cacheMap.Count / CacheData.PerTaskConfigSize;
                cache.TaskId = taskId;

                cacheMap[key] = cache;
            }

            foreach (var item in callBacks) cache.AddListener(item);

            return Task.CompletedTask;
        }

        protected override Task RemoveListenerInner(string dataId, string group, string tenant, Action<string> callBack)
        {
            tenant = string.IsNullOrWhiteSpace(tenant) ? _options.Namespace : tenant;
            group = string.IsNullOrWhiteSpace(group) ? ConstValue.DefaultGroup : group;

            string key = Nacos.V2.Config.GroupKey.GetKeyTenant(dataId, group, tenant);

            var cache = GetCache(dataId, group, tenant);

            if (cache != null)
            {
                cache.RemoveListener(callBack);
                if (cache.Listeners.Count <= 0) cacheMap.Remove(key);
            }

            return Task.CompletedTask;
        }

        private CacheData GetCache(string dataId, string group, string tenant)
            => cacheMap.TryGetValue(GroupKey.GetKeyTenant(dataId, group, tenant), out var data) ? data : null;

        private RpcClient EnsureRpcClient(string taskId)
        {
            Dictionary<string, string> labels = GetLabels();
            Dictionary<string, string> newlabels = new Dictionary<string, string>(labels);
            newlabels["taskId"] = taskId;

            RpcClient rpcClient = RpcClientFactory
                    .CreateClient("config-" + taskId + "-" + uuid, new RemoteConnectionType(RemoteConnectionType.GRPC), newlabels);

            if (rpcClient.IsWaitInited())
            {
                InitHandlerRpcClient(rpcClient);

                rpcClient.Start();
            }

            return rpcClient;
        }

        private void InitHandlerRpcClient(RpcClient rpcClientInner)
        {
            rpcClientInner.RegisterServerPushResponseHandler(new ConfigRpcServerRequestHandler(cacheMap));
            rpcClientInner.RegisterConnectionListener(new ConfigRpcConnectionEventListener(rpcClientInner, cacheMap));

            rpcClientInner.Init(new ConfigRpcServerListFactory(_serverListManager));
        }

        public class ConfigRpcServerRequestHandler : IServerRequestHandler
        {
            private Dictionary<string, CacheData> _cacheMap;

            public ConfigRpcServerRequestHandler(Dictionary<string, CacheData> map)
            {
                this._cacheMap = map;
            }

            public CommonResponse RequestReply(Payload payload, IClientStreamWriter<Payload> streamWriter)
            {
                throw new NotImplementedException();
            }

            public CommonResponse RequestReply(CommonRequest request, CommonRequestMeta meta)
            {
                if (request is ConfigChangeNotifyRequest)
                {
                    var configChangeNotifyRequest = (ConfigChangeNotifyRequest)request;

                    string groupKey = GroupKey.GetKeyTenant(configChangeNotifyRequest.DataId, configChangeNotifyRequest.Group, configChangeNotifyRequest.Tenant);

                    if (_cacheMap.TryGetValue(groupKey, out var cacheData))
                    {
                        if (configChangeNotifyRequest.ContentPush
                            && cacheData.LastModifiedTs < configChangeNotifyRequest.LastModifiedTs)
                        {
                        }

                        cacheData.IsListenSuccess = false;
                    }

                    Console.WriteLine("Config RequestReply => {0}", request.ToJsonString());

                    return new ConfigChangeNotifyResponse();
                }

                return null;
            }
        }

        public class ConfigRpcConnectionEventListener : IConnectionEventListener
        {
            private readonly RpcClient _rpcClient;
            private readonly Dictionary<string, CacheData> _cacheMap;

            public ConfigRpcConnectionEventListener(RpcClient rpcClientInner, Dictionary<string, CacheData> cacheMap)
            {
                this._rpcClient = rpcClientInner;
                this._cacheMap = cacheMap;
            }

            public void OnConnected()
            {
            }

            public void OnDisConnected()
            {
                if (_rpcClient.GetLabels().TryGetValue("taskId", out var taskId))
                {
                    var values = _cacheMap.Values;

                    foreach (var cacheData in values)
                    {
                        if (cacheData.TaskId.Equals(Convert.ToInt32(taskId)))
                        {
                            cacheData.IsListenSuccess = false;
                            continue;
                        }

                        cacheData.IsListenSuccess = false;
                    }
                }
            }
        }

        public class ConfigRpcServerListFactory : IServerListFactory
        {
            private readonly IServerListManager _serverListManager;

            public ConfigRpcServerListFactory(IServerListManager serverListManager)
            {
                this._serverListManager = serverListManager;
            }

            public string GenNextServer() => _serverListManager.GetNextServerAddr();

            public string GetCurrentServer() => _serverListManager.GetCurrentServerAddr();

            public List<string> GetServerList() => _serverListManager.GetServerUrls();
        }

        private Dictionary<string, string> GetLabels()
        {
            var labels = new Dictionary<string, string>(2)
            {
                [RemoteConstants.LABEL_SOURCE] = RemoteConstants.LABEL_SOURCE_SDK,
                [RemoteConstants.LABEL_MODULE] = RemoteConstants.LABEL_MODULE_CONFIG
            };
            return labels;
        }

        private async Task<CommonResponse> RequestProxy(RpcClient rpcClientInner, CommonRequest request)
        {
            // TODO: 1. security headers, spas headers
            // TODO: 2. limiter
            return await rpcClientInner.Request(request);
        }

        private RpcClient GetOneRunningClient()
        {
            return EnsureRpcClient("0");
        }
    }
}
