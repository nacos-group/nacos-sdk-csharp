namespace Nacos.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Config.Abst;
    using Nacos.Exceptions;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConfiggRpcTransportClient : AbstConfigTransportClient
    {
        private ILogger _logger;

        private readonly Grpc.Core.ChannelBase _channel;
        private readonly Remote.GRpc.GrpcSdkClient _sdkClient;
        private Dictionary<string, CacheData> cacheMap = new Dictionary<string, CacheData>();

        private readonly object _lock = new object();

        private Timer _configListenTimer;

        public ConfiggRpcTransportClient(
            ILoggerFactory loggerFactory,
            IOptionsMonitor<NacosOptions> optionAccs)
        {
            _logger = loggerFactory.CreateLogger<GrpcConfigClient>();
            _options = optionAccs.CurrentValue;

            _sdkClient = new Remote.GRpc.GrpcSdkClient("config");
            _channel = _sdkClient.ConnectToServer(_options.ServerAddresses[0]);

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
                var request = new Nacos.Config.Requests.ConfigPublishRequest(dataId, group, tenant, content);
                request.PutAdditonalParam("tag", tag);
                request.PutAdditonalParam("appName", appName);
                request.PutAdditonalParam("betaIps", betaIps);

                var payload = Remote.GRpc.GrpcUtils.Convert(request, new Remote.GRpc.RequestMeta
                {
                    Type = Remote.GRpc.GrpcRequestType.Config_Publish,
                    ClientIp = "",
                    ClientPort = 80,
                    ClientVersion = ConstValue.ClientVersion
                });

                var client = new Nacos.Request.RequestClient(_channel);

                var result = await client.requestAsync(payload);

#if DEBUG
                System.Diagnostics.Trace.WriteLine($"{Remote.GRpc.GrpcRequestType.Config_Publish} return {result.Body.Value.ToStringUtf8()}, {result.Metadata.ToJsonString()}");
#endif

                var resp = result.Body.Value.ToStringUtf8().ToObj<Nacos.Remote.CommonResponse>();

                return resp.IsSuccess();
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
                var request = new Nacos.Config.Requests.ConfigQueryRequest(dataId, group, tenant);
                request.PutHeader("notify", notify.ToString());

                var payload = Remote.GRpc.GrpcUtils.Convert(request, new Remote.GRpc.RequestMeta
                {
                    Type = Remote.GRpc.GrpcRequestType.Config_Get,
                    ClientIp = "",
                    ClientPort = 80,
                    ClientVersion = ConstValue.ClientVersion
                });

                var client = new Nacos.Request.RequestClient(_channel);

                var result = await client.requestAsync(payload);

#if DEBUG
                System.Diagnostics.Trace.WriteLine($"{Remote.GRpc.GrpcRequestType.Config_Get} return {result.Body.Value.ToStringUtf8()}, {result.Metadata.ToJsonString()}");
#endif
                var resp = result.Body.Value.ToStringUtf8().ToObj<Nacos.Config.Requests.ConfigQueryResponse>();

                var ct = new List<string>();

                if (resp.IsSuccess())
                {
                    // TODO snapshot
                    ct.Add(resp.Content);
                    ct.Add(string.IsNullOrWhiteSpace(resp.ContentType) ? resp.ContentType : "text");
                    return ct;
                }
                else if (resp.ErrorCode.Equals(Nacos.Config.Requests.ConfigQueryResponse.CONFIG_NOT_FOUND))
                {
                    // TODO snapshot
                    return ct;
                }
                else if (resp.ErrorCode.Equals(Nacos.Config.Requests.ConfigQueryResponse.CONFIG_QUERY_CONFLICT))
                {
                    _logger.LogError(
                        "[{0}] [sub-server-error] get server config being modified concurrently, dataId={1}, group={2}, tenant={3}",
                        GetName(), dataId, group, tenant);
                    throw new NacosException(Nacos.ConstValue.CONFLICT, $"data being modified, dataId={dataId},group={group},tenant={tenant}");
                }
                else
                {
                    _logger.LogError(
                       "[{0}] [sub-server-error]  dataId={1}, group={2}, tenant={3}, code={4}",
                       GetName(), dataId, group, tenant, resp.ToJsonString());
                    throw new NacosException(resp.ErrorCode, $"http error, code={resp.ErrorCode}, dataId={dataId},group={group},tenant={tenant}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{0}] [sub-server-error] ", GetName());
                throw;
            }
        }

        protected override async Task<bool> RemoveConfigInner(string dataId, string group, string tenant, string tag)
        {
            try
            {
                var request = new Nacos.Config.Requests.ConfigRemoveRequest(dataId, group, tenant, tag);

                var payload = Remote.GRpc.GrpcUtils.Convert(request, new Remote.GRpc.RequestMeta
                {
                    Type = Remote.GRpc.GrpcRequestType.Config_Remove,
                    ClientIp = "",
                    ClientPort = 80,
                    ClientVersion = ConstValue.ClientVersion
                });

                var client = new Nacos.Request.RequestClient(_channel);

                var result = await client.requestAsync(payload);

#if DEBUG
                System.Diagnostics.Trace.WriteLine($"{Remote.GRpc.GrpcRequestType.Config_Remove} return {result.Body.Value.ToStringUtf8()}, {result.Metadata.ToJsonString()}");
#endif
                var resp = result.Body.Value.ToStringUtf8().ToObj<Nacos.Remote.CommonResponse>();

                return resp.IsSuccess();
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
                try
                {
                    foreach (var task in listenCachesMap)
                    {
                        var taskId = task.Key;
                        var listenCaches = task.Value;

                        var request = new Nacos.Config.Requests.ConfigBatchListenRequest() { Listen = true };

                        foreach (var item in listenCaches)
                            request.AddConfigListenContext(item.Tenant, item.Group, item.DataId, item.Md5);

                        if (request.ConfigListenContexts != null && request.ConfigListenContexts.Any())
                        {
                            var payload = Remote.GRpc.GrpcUtils.Convert(request, new Remote.GRpc.RequestMeta
                            {
                                Type = Remote.GRpc.GrpcRequestType.Config_Listen,
                                ClientVersion = ConstValue.ClientVersion
                            });

                            var client = new Nacos.Request.RequestClient(_channel);

                            var result = await client.requestAsync(payload);

#if DEBUG
                            System.Diagnostics.Trace.WriteLine($"{Remote.GRpc.GrpcRequestType.Config_Listen} return {result.Body.Value.ToStringUtf8()}, {result.Metadata.ToJsonString()}");
#endif
                            var resp = result.Body.Value.ToStringUtf8().ToObj<Nacos.Config.Requests.ConfigChangeBatchListenResponse>();

                            if (resp != null && resp.IsSuccess())
                            {
                                HashSet<string> changeKeys = new HashSet<string>();

                                if (resp.ChangedConfigs != null && resp.ChangedConfigs.Any())
                                {
                                    foreach (var item in resp.ChangedConfigs)
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
                                                cached.Listeners.ForEach(x => x.Invoke(ct[0]));
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
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "async listen config change error ");
                }
            }

            if (removeListenCachesMap != null && removeListenCachesMap.Any())
            {
                try
                {
                    foreach (var task in removeListenCachesMap)
                    {
                        var taskId = task.Key;
                        var removeListenCaches = task.Value;

                        var request = new Nacos.Config.Requests.ConfigBatchListenRequest { Listen = false };

                        foreach (var item in removeListenCaches)
                            request.AddConfigListenContext(item.Tenant, item.Group, item.DataId, item.Md5);

                        if (request.ConfigListenContexts != null && request.ConfigListenContexts.Any())
                        {
                            var payload = Remote.GRpc.GrpcUtils.Convert(request, new Remote.GRpc.RequestMeta
                            {
                                Type = Remote.GRpc.GrpcRequestType.Config_Listen,
                                ClientVersion = ConstValue.ClientVersion
                            });

                            var client = new Nacos.Request.RequestClient(_channel);

                            var result = await client.requestAsync(payload);

#if DEBUG
                            System.Diagnostics.Trace.WriteLine($"{Remote.GRpc.GrpcRequestType.Config_Listen} return {result.Body.Value.ToStringUtf8()}, {result.Metadata.ToJsonString()}");
#endif
                            var resp = result.Body.Value.ToStringUtf8().ToObj<Nacos.Config.Requests.ConfigChangeBatchListenResponse>();

                            if (resp != null && resp.IsSuccess())
                            {
                                // do remove
                                Console.WriteLine("do remove");
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

        protected override Task AddListenerInner(string dataId, string group, string tenant, List<Action<string>> callBacks)
        {
            tenant = string.IsNullOrWhiteSpace(tenant) ? _options.Namespace : tenant;
            group = string.IsNullOrWhiteSpace(group) ? ConstValue.DefaultGroup : group;

            var cache = GetCache(dataId, group, tenant);
            if (cache != null) return Task.CompletedTask;

            string key = Nacos.Config.GroupKey.GetKeyTenant(dataId, group, tenant);

            if (cacheMap.TryGetValue(key, out var cached)) return Task.CompletedTask;

            cache = new CacheData { DataId = dataId, Group = group, Tenant = tenant };

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

            string key = Nacos.Config.GroupKey.GetKeyTenant(dataId, group, tenant);

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
    }
}
