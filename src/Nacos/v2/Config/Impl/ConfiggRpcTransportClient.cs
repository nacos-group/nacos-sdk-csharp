namespace Nacos.Config.Impl
{
    using Grpc.Net.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Config.Abst;
    using Nacos.Exceptions;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConfiggRpcTransportClient : AbstConfigTransportClient
    {
        private ILogger _logger;

        private readonly GrpcChannel _channel;
        private readonly Remote.GRpc.GrpcSdkClient _sdkClient;

        private Timer _configListenTimer;

        public ConfiggRpcTransportClient(
            ILoggerFactory loggerFactory,
            IOptionsMonitor<NacosOptions> optionAccs)
        {
            _logger = loggerFactory.CreateLogger<GrpcConfigClient>();
            _options = optionAccs.CurrentValue;

            _sdkClient = new Remote.GRpc.GrpcSdkClient("config");
            _channel = _sdkClient.ConnectToServer(_options.ServerAddresses[0]);
        }

        protected override string GetNameInner() => "config_rpc_client";

        protected override string GetNamespaceInner()
        {
            throw new NotImplementedException();
        }

        protected override string GetTenantInner()
        {
            throw new NotImplementedException();
        }

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
            try
            {
                // TODO: Read from listen map
                var request = new Nacos.Config.Requests.ConfigBatchListenRequest();
                request.AddConfigListenContext("", "", "", "");

                var payload = Remote.GRpc.GrpcUtils.Convert(request, new Remote.GRpc.RequestMeta
                {
                    Type = Remote.GRpc.GrpcRequestType.Config_Listen,
                    ClientIp = "",
                    ClientPort = 80,
                    ClientVersion = ConstValue.ClientVersion
                });

                var client = new Nacos.Request.RequestClient(_channel);

                var result = await client.requestAsync(payload);

#if DEBUG
                System.Diagnostics.Trace.WriteLine($"{Remote.GRpc.GrpcRequestType.Config_Listen} return {result.Body.Value.ToStringUtf8()}, {result.Metadata.ToJsonString()}");
#endif
                var resp = result.Body.Value.ToStringUtf8().ToObj<Nacos.Remote.CommonResponse>();

                if (resp != null && resp.IsSuccess())
                {
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "async listen config change error ");
                throw;
            }
        }
    }
}
