namespace Nacos.V2.Naming.Remote.Grpc
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Remote;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using Nacos.Utilities;

    public class NamingGrpcClientProxy : INamingClientProxy, IDisposable
    {
        private readonly ILogger _logger;

        private string namespaceId;

        private string uuid;

        private long requestTimeout;

        private RpcClient rpcClient;

        private NamingGrpcConnectionEventListener namingGrpcConnectionEventListener;

        public NamingGrpcClientProxy(
            ILogger logger,
            string namespaceId,
            IServerListFactory serverListFactory,
            IOptionsMonitor<NacosSdkOptions> optionsMonitor,
            ServiceInfoHolder serviceInfoHolder)
        {
            this._logger = logger;
            this.namespaceId = namespaceId;
            this.uuid = Guid.NewGuid().ToString();

            // TODO
            this.requestTimeout = 3000L;

            Dictionary<string, string> labels = new Dictionary<string, string>()
            {
                { RemoteConstants.LABEL_SOURCE, RemoteConstants.LABEL_SOURCE_SDK },
                { RemoteConstants.LABEL_MODULE, RemoteConstants.LABEL_MODULE_NAMING },
            };

            this.rpcClient = RpcClientFactory.CreateClient(uuid, new RemoteConnectionType(RemoteConnectionType.GRPC), labels);

            this.namingGrpcConnectionEventListener = new NamingGrpcConnectionEventListener(_logger, this);

            Start(serverListFactory, serviceInfoHolder);
        }

        private void Start(IServerListFactory serverListFactory, ServiceInfoHolder serviceInfoHolder)
        {
            rpcClient.Init(serverListFactory);
            rpcClient.Start();
            rpcClient.RegisterServerPushResponseHandler(new NamingPushResponseHandler(serviceInfoHolder));
            rpcClient.RegisterConnectionListener(namingGrpcConnectionEventListener);
        }

        public Task CreateService(Service service, AbstractSelector selector)
        {
            return Task.CompletedTask;
        }

        public Task<bool> DeleteService(string serviceName, string groupName)
        {
            return Task.FromResult(false);
        }

        public async Task DeregisterService(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[DEREGISTER-SERVICE] {0} registering service {1} with instance {2}", namespaceId, serviceName, instance);

            var request = new InstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.DE_REGISTER_INSTANCE, instance);

            await RequestToServer<CommonResponse>(request);
            namingGrpcConnectionEventListener.RemoveInstanceForRedo(serviceName, groupName, instance);
        }

        public async Task<ListView<string>> GetServiceList(int pageNo, int pageSize, string groupName, AbstractSelector selector)
        {
            var request = new ServiceListRequest(namespaceId, groupName, pageNo, pageSize);

            if (selector != null && selector.Type.Equals("label")) request.Selector = selector.ToJsonString();

            var response = await RequestToServer<ServiceListResponse>(request);

            var result = new ListView<string>(response.Count, response.ServiceNames);
            return result;
        }

        public async Task<Dtos.ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, int udpPort, bool healthyOnly)
        {
            var request = new ServiceQueryRequest(namespaceId, serviceName, groupName)
            {
                Cluster = clusters,
                HealthyOnly = healthyOnly,
                UdpPort = udpPort
            };

            var response = await RequestToServer<QueryServiceResponse>(request);
            return response.ServiceInfo;
        }

        public Task<Service> QueryService(string serviceName, string groupName)
        {
            return Task.FromResult<Service>(null);
        }

        public async Task RegisterServiceAsync(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[REGISTER-SERVICE] {0} registering service {1} with instance {2}", namespaceId, serviceName, instance);

            var request = new InstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.REGISTER_INSTANCE, instance);

            await RequestToServer<CommonResponse>(request);

            namingGrpcConnectionEventListener.CacheInstanceForRedo(serviceName, groupName, instance);
        }

        public bool ServerHealthy() => rpcClient.IsRunning();

        public async Task<Dtos.ServiceInfo> Subscribe(string serviceName, string groupName, string clusters)
        {
            var request = new SubscribeServiceRequest(namespaceId, serviceName, groupName, clusters, true);
            var response = await RequestToServer<SubscribeServiceResponse>(request);

            namingGrpcConnectionEventListener.CacheSubscriberForRedo(NamingUtils.GetGroupedName(serviceName, groupName), clusters);
            return response.ServiceInfo;
        }

        public async Task Unsubscribe(string serviceName, string groupName, string clusters)
        {
            var request = new SubscribeServiceRequest(namespaceId, serviceName, groupName, clusters, true);
            await RequestToServer<SubscribeServiceResponse>(request);

            namingGrpcConnectionEventListener.RemoveSubscriberForRedo(NamingUtils.GetGroupedName(serviceName, groupName), clusters);
        }

        public Task UpdateBeatInfo(List<Instance> modifiedInstances)
        {
            return Task.CompletedTask;
        }

        public Task UpdateInstance(string serviceName, string groupName, Instance instance)
        {
            return Task.CompletedTask;
        }

        public Task UpdateService(Service service, AbstractSelector selector)
        {
            return Task.CompletedTask;
        }

        private async Task<T> RequestToServer<T>(CommonRequest request)
            where T : CommonResponse
        {
            try
            {
                CommonResponse response =
                        requestTimeout < 0
                        ? await rpcClient.Request(request)
                        : await rpcClient.Request(request, requestTimeout);

                if (response.ResultCode != 200)
                {
                    throw new NacosException(response.ErrorCode, response.Message);
                }

                if (response is T)
                {
                    return (T)response;
                }

                _logger?.LogError("Server return unexpected response '{}', expected response should be '{}'", response.GetType().Name, typeof(T).Name);
            }
            catch (Exception e)
            {
                throw new NacosException(NacosException.SERVER_ERROR, "Request nacos server failed: " + e.Message);
            }

            throw new NacosException(NacosException.SERVER_ERROR, "Server return invalid response");
        }

        public void Dispose() => rpcClient.Dispose();
    }
}
