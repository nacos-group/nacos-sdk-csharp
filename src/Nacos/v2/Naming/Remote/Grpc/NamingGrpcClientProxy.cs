namespace Nacos.Naming.Remote.Grpc
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Exceptions;
    using Nacos.Naming.Cache;
    using Nacos.Naming.Dtos;
    using Nacos.Remote;
    using Nacos.Remote.Requests;
    using Nacos.Remote.Responses;

    public class NamingGrpcClientProxy : INamingClientProxy
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
            IOptionsMonitor<NacosOptions> optionsMonitor,
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

            this.namingGrpcConnectionEventListener = new NamingGrpcConnectionEventListener();

            Start(serverListFactory, serviceInfoHolder);
        }

        private void Start(IServerListFactory serverListFactory, ServiceInfoHolder serviceInfoHolder)
        {
            rpcClient.Init(serverListFactory);
            rpcClient.Start();
            rpcClient.RegisterServerPushResponseHandler(new NamingPushResponseHandler());
            rpcClient.RegisterConnectionListener(namingGrpcConnectionEventListener);
        }

        public Task CreateService(Service service, AbstractSelector selector)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteService(string serviceName, string groupName)
        {
            throw new NotImplementedException();
        }

        public async Task DeregisterService(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[DEREGISTER-SERVICE] {0} registering service {1} with instance {2}", namespaceId, serviceName, instance);

            var request = new InstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.DE_REGISTER_INSTANCE, instance);

            // TODO
            await RequestToServer<CommonResponse>(request);
        }

        public List<string> GetServiceList(int pageNo, int pageSize, string groupName, AbstractSelector selector)
        {
            throw new NotImplementedException();
        }

        public async Task<Dtos.ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, int udpPort, bool healthyOnly)
        {
            ServiceQueryRequest request = new ServiceQueryRequest(namespaceId, serviceName, groupName)
            {
                Cluster = clusters,
                HealthyOnly = healthyOnly,
                UdpPort = udpPort
            };

            var response = await RequestToServer<QueryServiceResponse>(request);
            return response.ServiceInfo;
        }

        public Service QueryService(string serviceName, string groupName)
        {
            throw new NotImplementedException();
        }

        public async Task RegisterServiceAsync(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[REGISTER-SERVICE] {0} registering service {1} with instance {2}", namespaceId, serviceName, instance);

            var request = new InstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.REGISTER_INSTANCE, instance);

            // TODO
            await RequestToServer<CommonResponse>(request);
        }

        public bool ServerHealthy()
        {
            throw new NotImplementedException();
        }

        public Dtos.ServiceInfo Subscribe(string serviceName, string groupName, string clusters)
        {
            throw new NotImplementedException();
        }

        public Task Unsubscribe(string serviceName, string groupName, string clusters)
        {
            throw new NotImplementedException();
        }

        public Task UpdateBeatInfo(List<Instance> modifiedInstances)
        {
            throw new NotImplementedException();
        }

        public Task UpdateInstance(string serviceName, string groupName, Instance instance)
        {
            return Task.CompletedTask;
        }

        public Task UpdateService(Service service, AbstractSelector selector)
        {
            throw new NotImplementedException();
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
    }
}
