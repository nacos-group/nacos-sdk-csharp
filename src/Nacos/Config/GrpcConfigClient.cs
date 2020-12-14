namespace Nacos.Config
{
    using Grpc.Net.Client;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Exceptions;
    using System.Threading.Tasks;

    public class GrpcConfigClient : INacosConfigClient
    {
        private ILogger _logger;
        private NacosOptions _options;

        private readonly GrpcChannel _channel;
        private readonly Remote.GRpc.GrpcSdkClient _sdkClient;

        public GrpcConfigClient(
            ILoggerFactory loggerFactory,
            IOptionsMonitor<NacosOptions> optionAccs)
        {
            _logger = loggerFactory.CreateLogger<GrpcConfigClient>();
            _options = optionAccs.CurrentValue;

            _sdkClient = new Remote.GRpc.GrpcSdkClient("config");
            _channel = _sdkClient.ConnectToServer(_options.ServerAddresses[0]);
        }

        public string Name => "grpc";

        public Task AddListenerAsync(AddListenerRequest request)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> GetConfigAsync(GetConfigRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? "" : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var payload = Remote.GRpc.GrpcUtils.Convert<GetConfigRequest>(request, new Remote.GRpc.RequestMeta
            {
                 Type = Remote.GRpc.GrpcRequestType.Config_Get,
                 ClientIp = "",
                 ClientPort = 80,
                 ClientVersion = ConstValue.ClientVersion
            });

            var client = new Nacos.Request.RequestClient(_channel);

            var res = await client.requestAsync(payload);

            System.Diagnostics.Trace.WriteLine($"GetConfigAsync return {res.Body.Value.ToStringUtf8()}, {Newtonsoft.Json.JsonConvert.SerializeObject(res.Metadata)}");

            var raw = Remote.GRpc.GrpcUtils.Convert(res);

            return raw;
        }

        public Task<string> GetServerStatus()
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> PublishConfigAsync(PublishConfigRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? "" : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var payload = Remote.GRpc.GrpcUtils.Convert<PublishConfigRequest>(request, new Remote.GRpc.RequestMeta
            {
                Type = Remote.GRpc.GrpcRequestType.Config_Publish,
                ClientIp = "",
                ClientPort = 80,
                ClientVersion = ConstValue.ClientVersion
            });

            var client = new Nacos.Request.RequestClient(_channel);

            var res = await client.requestAsync(payload);

            System.Diagnostics.Trace.WriteLine($"PublishConfigAsync return {res.Body.Value.ToStringUtf8()}, {Newtonsoft.Json.JsonConvert.SerializeObject(res.Metadata)}");

            return true;
        }

        public async Task<bool> RemoveConfigAsync(RemoveConfigRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? "" : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var payload = Remote.GRpc.GrpcUtils.Convert<RemoveConfigRequest>(request, new Remote.GRpc.RequestMeta
            {
                Type = Remote.GRpc.GrpcRequestType.Config_Remove,
                ClientIp = "",
                ClientPort = 80,
                ClientVersion = ConstValue.ClientVersion
            });

            var client = new Nacos.Request.RequestClient(_channel);

            var res = await client.requestAsync(payload);

            System.Diagnostics.Trace.WriteLine($"PublishConfigAsync return {res.Body.Value.ToStringUtf8()}, {Newtonsoft.Json.JsonConvert.SerializeObject(res.Metadata)}");

            return true;
        }

        public Task RemoveListenerAsync(RemoveListenerRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
