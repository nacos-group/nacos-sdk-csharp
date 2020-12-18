namespace Nacos.Config
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Exceptions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GrpcConfigClient : INacosConfigClient
    {
        private ILogger _logger;
        private NacosOptions _options;
        private List<Listener> listeners = new List<Listener>();

        private readonly Nacos.Config.Abst.IConfigTransportClient _agent;

        public GrpcConfigClient(
            ILoggerFactory loggerFactory,
            IEnumerable<Nacos.Config.Abst.IConfigTransportClient> agents,
            IOptionsMonitor<NacosOptions> optionAccs)
        {
            _logger = loggerFactory.CreateLogger<GrpcConfigClient>();
            _options = optionAccs.CurrentValue;
            _agent = agents.Single(x => x.GetName().Equals("config_rpc_client"));
        }

        public string Name => "grpc";

        public async Task AddListenerAsync(AddListenerRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            if (string.IsNullOrWhiteSpace(request.Tenant)) request.Tenant = _options.Namespace;
            if (string.IsNullOrWhiteSpace(request.Group)) request.Group = ConstValue.DefaultGroup;

            request.CheckParam();

            await Task.Delay(1);
        }

        public async Task<string> GetConfigAsync(GetConfigRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var list = await _agent.QueryConfigAsync(request.DataId, request.Group, request.Tenant, 5000, true);

            return (list != null && list.Any()) ? list[0] : null;
        }

        public Task<string> GetServerStatus()
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> PublishConfigAsync(PublishConfigRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var res = await _agent.PublishConfigAsync(request.DataId, request.Group, request.Tenant, request.AppName, request.Tag, "", request.Content);

            return res;
        }

        public async Task<bool> RemoveConfigAsync(RemoveConfigRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var res = await _agent.RemoveConfigAsync(request.DataId, request.Group, request.Tenant, request.Tag);

            return res;
        }

        public Task RemoveListenerAsync(RemoveListenerRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
