namespace Nacos.Naming.Remote.Grpc
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos;
    using Nacos.Common;
    using Nacos.Exceptions;
    using Nacos.Logging;
    using Nacos.Naming.Cache;
    using Nacos.Naming.Dtos;
    using Nacos.Naming.Remote;
    using Nacos.Naming.Remote.Grpc.Redo;
    using Nacos.Naming.Utils;
    using Nacos.Remote;
    using Nacos.Remote.Requests;
    using Nacos.Remote.Responses;
    using Nacos.Security;
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class NamingGrpcClientProxy : INamingGrpcClientProxy
    {
        private readonly ILogger _logger = NacosLogManager.CreateLogger<NamingGrpcClientProxy>();

        private string namespaceId;

        private string uuid;

        private long requestTimeout;

        private RpcClient rpcClient;

        private ISecurityProxy _securityProxy;

        private NacosSdkOptions _options;

        private NamingGrpcRedoService _redoService;

        public NamingGrpcClientProxy(
            ISecurityProxy securityProxy,
            IServerListFactory serverListFactory,
            IOptions<NacosSdkOptions> optionsAccs,
            ServiceInfoHolder serviceInfoHolder)
        {
            uuid = Guid.NewGuid().ToString();
            _options = optionsAccs.Value;
            _securityProxy = securityProxy;
            namespaceId = string.IsNullOrWhiteSpace(_options.Namespace) ? Constants.DEFAULT_NAMESPACE_ID : _options.Namespace;
            requestTimeout = _options.DefaultTimeOut > 0 ? _options.DefaultTimeOut : 3000L;

            Dictionary<string, string> labels = new Dictionary<string, string>()
            {
                { RemoteConstants.LABEL_SOURCE, RemoteConstants.LABEL_SOURCE_SDK },
                { RemoteConstants.LABEL_MODULE, RemoteConstants.LABEL_MODULE_NAMING },
            };

            rpcClient = RpcClientFactory.CreateClient(uuid, RemoteConnectionType.GRPC, labels, _options.TLSConfig);
            _redoService = new NamingGrpcRedoService(this);

            Start(serverListFactory, serviceInfoHolder);
        }

        private void Start(IServerListFactory serverListFactory, ServiceInfoHolder serviceInfoHolder)
        {
            rpcClient.Init(serverListFactory);
            rpcClient.RegisterConnectionListener(_redoService);
            rpcClient.RegisterServerPushResponseHandler(new NamingPushRequestHandler(serviceInfoHolder));
            rpcClient.Start();
        }

        public Task CreateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        public Task<bool> DeleteService(string serviceName, string groupName) => Task.FromResult(false);

        public async Task DeregisterService(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[DEREGISTER-SERVICE] {0} deregistering service {1} with instance {2}", namespaceId, serviceName, instance);

            _redoService.InstanceDeregister(serviceName, groupName);
            await DoDeregisterService(serviceName, groupName, instance).ConfigureAwait(false);
        }

        public async Task DoDeregisterService(string serviceName, string groupName, Instance instance)
        {
            var request = new InstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.DE_REGISTER_INSTANCE, instance);
            await RequestToServer<CommonResponse>(request).ConfigureAwait(false);
            _redoService.RemoveInstanceForRedo(serviceName, groupName);
        }

        public async Task<ListView<string>> GetServiceList(int pageNo, int pageSize, string groupName, AbstractSelector selector)
        {
            var request = new ServiceListRequest(namespaceId, groupName, pageNo, pageSize);

            if (selector != null && selector.Type.Equals("label")) request.Selector = selector.ToJsonString();

            var response = await RequestToServer<ServiceListResponse>(request).ConfigureAwait(false);

            var result = new ListView<string>(response.Count, response.ServiceNames);
            return result;
        }

        public async Task<ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, int udpPort, bool healthyOnly)
        {
            var request = new ServiceQueryRequest(namespaceId, serviceName, groupName)
            {
                Cluster = clusters,
                HealthyOnly = healthyOnly,
                UdpPort = udpPort
            };

            var response = await RequestToServer<QueryServiceResponse>(request).ConfigureAwait(false);
            return response.ServiceInfo;
        }

        public Task<Service> QueryService(string serviceName, string groupName) => Task.FromResult<Service>(null);

        public async Task RegisterServiceAsync(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[REGISTER-SERVICE] {0} registering service {1} with instance {2}", namespaceId, serviceName, instance);

            _redoService.CacheInstanceForRedo(serviceName, groupName, instance);
            await DoRegisterService(serviceName, groupName, instance).ConfigureAwait(false);
        }

        public async Task BatchRegisterServiceAsync(string serviceName, string groupName, List<Instance> instances)
        {
            _redoService.CacheInstanceForRedo(serviceName, groupName, instances);
            await DoBatchRegisterService(serviceName, groupName, instances).ConfigureAwait(false);
        }

        private async Task DoBatchRegisterService(string serviceName, string groupName, List<Instance> instances)
        {
            var request = new BatchInstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.BATCH_REGISTER_INSTANCE, instances);
            await RequestToServer<CommonResponse>(request).ConfigureAwait(false);
            _redoService.InstanceRegistered(serviceName, groupName);
        }

        public async Task DoRegisterService(string serviceName, string groupName, Instance instance)
        {
            var request = new InstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.REGISTER_INSTANCE, instance);
            await RequestToServer<CommonResponse>(request).ConfigureAwait(false);
            _redoService.InstanceRegistered(serviceName, groupName);
        }

        public bool ServerHealthy() => rpcClient.IsRunning();

        public async Task<ServiceInfo> Subscribe(string serviceName, string groupName, string clusters)
        {
            _logger?.LogDebug("[GRPC-SUBSCRIBE] service:{0}, group:{1}, cluster:{2} ", serviceName, groupName, clusters);
            _redoService.CacheSubscriberForRedo(serviceName, groupName, clusters);
            return await DoSubscribe(serviceName, groupName, clusters).ConfigureAwait(false);
        }

        public async Task<ServiceInfo> DoSubscribe(string serviceName, string groupName, string clusters)
        {
            var request = new SubscribeServiceRequest(namespaceId, serviceName, groupName, clusters, true);
            var response = await RequestToServer<SubscribeServiceResponse>(request).ConfigureAwait(false);
            _redoService.SubscriberRegistered(serviceName, groupName, clusters);
            return response.ServiceInfo;
        }

        public async Task Unsubscribe(string serviceName, string groupName, string clusters)
        {
            _logger?.LogDebug("[GRPC-UNSUBSCRIBE] service:{0}, group:{1}, cluster:{2} ", serviceName, groupName, clusters);
            _redoService.SubscriberDeregister(serviceName, groupName, clusters);
            await DoUnsubscribe(serviceName, groupName, clusters).ConfigureAwait(false);
        }

        public async Task DoUnsubscribe(string serviceName, string groupName, string clusters)
        {
            var request = new SubscribeServiceRequest(namespaceId, serviceName, groupName, clusters, false);
            await RequestToServer<SubscribeServiceResponse>(request).ConfigureAwait(false);
            _redoService.RemoveSubscriberForRedo(serviceName, groupName, clusters);
        }

        public Task UpdateBeatInfo(List<Instance> modifiedInstances) => Task.CompletedTask;

        public Task UpdateInstance(string serviceName, string groupName, Instance instance) => Task.CompletedTask;

        public Task UpdateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        public bool IsEnable() => rpcClient.IsRunning();

        private async Task<T> RequestToServer<T>(AbstractNamingRequest request)
            where T : CommonResponse
        {
            try
            {
                request.PutAllHeader(GetSecurityHeaders(request.Namespace, request.GroupName, request.ServiceName));
                request.PutAllHeader(GetSpasHeaders(NamingUtils.GetGroupedNameOptional(request.ServiceName, request.GroupName)));

                CommonResponse response =
                        requestTimeout < 0
                        ? await rpcClient.Request(request).ConfigureAwait(false)
                        : await rpcClient.Request(request, requestTimeout).ConfigureAwait(false);

                if (response == null)
                {
                    throw new NacosException(NacosException.SERVER_ERROR, "Request nacos server failed: RequestToServer<T>");
                }

                if (response.ResultCode != 200)
                {
                    throw new NacosException(response.ErrorCode, response.Message);
                }

                if (response is T)
                {
                    return (T)response;
                }

                _logger?.LogError("Server return unexpected response '{0}', expected response should be '{1}'", response.GetType().Name, typeof(T).Name);
            }
            catch (NacosException e)
            {
                throw new NacosException(e.ErrorCode, $"Request nacos server failed: {e.ErrorCode}, {e.Message}");
            }
            catch (Exception e)
            {
                throw new NacosException(NacosException.SERVER_ERROR, $"Request nacos server failed: {e.Message}");
            }

            throw new NacosException(NacosException.SERVER_ERROR, "Server return invalid response");
        }

        public void Dispose() => rpcClient.Dispose();

        private Dictionary<string, string> GetSecurityHeaders(string @namespace, string group, string serviceName)
        {
            var resource = new Nacos.Auth.RequestResource("naming", @namespace, group, serviceName);
            var result = _securityProxy.GetIdentityContext(resource);
            result[CommonParams.APP_FILED] = AppDomain.CurrentDomain.FriendlyName;
            return result;
        }

        private Dictionary<string, string> GetSpasHeaders(string serviceName)
        {
            var result = new Dictionary<string, string>(2);

            result[CommonParams.APP_FILED] = AppDomain.CurrentDomain.FriendlyName;

            if (string.IsNullOrWhiteSpace(_options.AccessKey)
                && string.IsNullOrWhiteSpace(_options.SecretKey))
                return result;

            string signData = !string.IsNullOrWhiteSpace(serviceName)
                ? DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() + CommonParams.SEPARATOR + serviceName
                : DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            string signature = HashUtil.GetHMACSHA1(signData, _options.SecretKey);
            result[CommonParams.SIGNATURE_FILED] = signature;
            result[CommonParams.DATA_FILED] = signData;
            result[CommonParams.AK_FILED] = _options.AccessKey;

            return result;
        }

        public Task<bool> IsSubscribed(string serviceName, string groupName, string clusters)
            => Task.FromResult(_redoService.IsSubscriberRegistered(serviceName, groupName, clusters));
    }
}
