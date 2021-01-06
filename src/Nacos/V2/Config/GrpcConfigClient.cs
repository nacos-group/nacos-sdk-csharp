namespace Nacos.V2.Config
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GrpcConfigClient : INacosConfigClient
    {
        private ILogger _logger;
        private NacosSdkOptions _options;
        private List<Listener> listeners = new List<Listener>();

        private readonly Abst.IConfigTransportClient _agent;

        public GrpcConfigClient(
            ILoggerFactory loggerFactory,
            IEnumerable<Abst.IConfigTransportClient> agents,
            IOptionsMonitor<NacosSdkOptions> optionAccs)
        {
            _logger = loggerFactory.CreateLogger<GrpcConfigClient>();
            _options = optionAccs.CurrentValue;
            _agent = agents.Single(x => x.GetName().Equals("config_rpc_client"));
        }

        public string Name => "grpc";

        public Task AddListenerAsync(AddListenerRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            if (string.IsNullOrWhiteSpace(request.Tenant)) request.Tenant = _options.Namespace;
            if (string.IsNullOrWhiteSpace(request.Group)) request.Group = ConstValue.DefaultGroup;

            request.CheckParam();

            /*return _agent.AddListenerAsync(request.DataId, request.Group, request.Tenant, request.Callbacks);*/
            return Task.CompletedTask;
        }

        public async Task<string> GetConfigAsync(GetConfigRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            // read from local cache at first
            var content = await Impl.FileLocalConfigInfoProcessor.GetFailoverAsync(_agent.GetName(), request.DataId, request.Group, request.Tenant);

            if (!string.IsNullOrWhiteSpace(content))
            {
                // TODO truncate content
                _logger.LogWarning("[{0}] [get-config] get failover ok, dataId={1}, group={2}, tenant={3}, config={4}", _agent.GetName(), request.DataId, request.Group, request.Tenant, content);

                return content;
            }

            try
            {
                var list = await _agent.QueryConfigAsync(request.DataId, request.Group, request.Tenant, 5000, true);

                return (list != null && list.Any()) ? list[0] : null;
            }
            catch (NacosException e) when (e.ErrorCode == NacosException.NO_RIGHT)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "[{0}] [get-config] get from server error, dataId={1}, group={2}, tenant={3}, msg={4}",
                    _agent.GetName(), request.DataId, request.Group, request.Tenant, ex.Message);
            }

            _logger.LogWarning(
                "[{}] [get-config] get snapshot ok, dataId={}, group={}, tenant={}, config={}",
                _agent.GetName(), request.DataId, request.Group, request.Tenant, content);

            content = await Impl.FileLocalConfigInfoProcessor.GetSnapshotAync(_agent.GetName(), request.DataId, request.Group, request.Tenant);
            return content;
        }

        public Task<string> GetServerStatus()
        {
            return Task.FromResult("UP");
        }

        public async Task<bool> PublishConfigAsync(PublishConfigRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var res = await _agent.PublishConfigAsync(request.DataId, request.Group, request.Tenant, request.AppName, request.Tag, "", request.Content);

            return res;
        }

        public async Task<bool> RemoveConfigAsync(RemoveConfigRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            request.Tenant = string.IsNullOrWhiteSpace(request.Tenant) ? _options.Namespace : request.Tenant;
            request.Group = string.IsNullOrWhiteSpace(request.Group) ? ConstValue.DefaultGroup : request.Group;

            request.CheckParam();

            var res = await _agent.RemoveConfigAsync(request.DataId, request.Group, request.Tenant, request.Tag);

            return res;
        }

        public Task RemoveListenerAsync(RemoveListenerRequest request)
        {
            if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

            if (string.IsNullOrWhiteSpace(request.Tenant)) request.Tenant = _options.Namespace;
            if (string.IsNullOrWhiteSpace(request.Group)) request.Group = ConstValue.DefaultGroup;

            request.CheckParam();

            /*return _agent.RemoveListenerAsync(request.DataId, request.Group, request.Tenant, null);*/
            return Task.CompletedTask;
        }
    }
}
