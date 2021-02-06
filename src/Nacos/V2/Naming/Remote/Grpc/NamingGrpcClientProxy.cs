namespace Nacos.V2.Naming.Remote.Grpc
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Common;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Remote;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using Nacos.V2.Security;
    using Nacos.V2.Utils;

    public class NamingGrpcClientProxy : INamingClientProxy, IDisposable
    {
        private readonly ILogger _logger;

        private string namespaceId;

        private string uuid;

        private long requestTimeout;

        private RpcClient rpcClient;

        private SecurityProxy _securityProxy;

        private NacosSdkOptions _options;

        private NamingGrpcConnectionEventListener namingGrpcConnectionEventListener;

        public NamingGrpcClientProxy(
            ILogger logger,
            string namespaceId,
            SecurityProxy securityProxy,
            IServerListFactory serverListFactory,
            NacosSdkOptions options,
            ServiceInfoHolder serviceInfoHolder)
        {
            this._logger = logger;
            this.namespaceId = namespaceId;
            this.uuid = Guid.NewGuid().ToString();
            this._options = options;
            this._securityProxy = securityProxy;

            // TODO
            this.requestTimeout = 5000L;

            Dictionary<string, string> labels = new Dictionary<string, string>()
            {
                { RemoteConstants.LABEL_SOURCE, RemoteConstants.LABEL_SOURCE_SDK },
                { RemoteConstants.LABEL_MODULE, RemoteConstants.LABEL_MODULE_NAMING },
            };

            this.rpcClient = RpcClientFactory.CreateClient(uuid, RemoteConnectionType.GRPC, labels);

            this.namingGrpcConnectionEventListener = new NamingGrpcConnectionEventListener(_logger, this);

            Start(serverListFactory, serviceInfoHolder);
        }

        private void Start(IServerListFactory serverListFactory, ServiceInfoHolder serviceInfoHolder)
        {
            rpcClient.Init(serverListFactory);
            rpcClient.Start();
            rpcClient.RegisterServerPushResponseHandler(new NamingPushRequestHandler(serviceInfoHolder));
            rpcClient.RegisterConnectionListener(namingGrpcConnectionEventListener);
        }

        public Task CreateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        public Task<bool> DeleteService(string serviceName, string groupName) => Task.FromResult(false);

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

        public async Task<ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, int udpPort, bool healthyOnly)
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

        public Task<Service> QueryService(string serviceName, string groupName) => Task.FromResult<Service>(null);

        public async Task RegisterServiceAsync(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[REGISTER-SERVICE] {0} registering service {1} with instance {2}", namespaceId, serviceName, instance);

            var request = new InstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.REGISTER_INSTANCE, instance);

            await RequestToServer<CommonResponse>(request);

            namingGrpcConnectionEventListener.CacheInstanceForRedo(serviceName, groupName, instance);
        }

        public bool ServerHealthy() => rpcClient.IsRunning();

        public async Task<ServiceInfo> Subscribe(string serviceName, string groupName, string clusters)
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

        public Task UpdateBeatInfo(List<Instance> modifiedInstances) => Task.CompletedTask;

        public Task UpdateInstance(string serviceName, string groupName, Instance instance) => Task.CompletedTask;

        public Task UpdateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        private async Task<T> RequestToServer<T>(AbstractNamingRequest request)
            where T : CommonResponse
        {
            try
            {
                request.PutAllHeader(GetSecurityHeaders());
                request.PutAllHeader(GetSpasHeaders(NamingUtils.GetGroupedNameOptional(request.ServiceName, request.GroupName)));

                CommonResponse response =
                        requestTimeout < 0
                        ? await rpcClient.Request(request)
                        : await rpcClient.Request(request, requestTimeout);

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
            catch (Exception e)
            {
                throw new NacosException(NacosException.SERVER_ERROR, "Request nacos server failed: " + e.Message);
            }

            throw new NacosException(NacosException.SERVER_ERROR, "Server return invalid response");
        }

        public void Dispose() => rpcClient.Dispose();

        private Dictionary<string, string> GetSecurityHeaders()
        {
            var result = new Dictionary<string, string>(1);
            if (_securityProxy.GetAccessToken().IsNotNullOrWhiteSpace())
            {
                result[Constants.ACCESS_TOKEN] = _securityProxy.GetAccessToken();
            }

            return result;
        }

        private Dictionary<string, string> GetSpasHeaders(string serviceName)
        {
            var result = new Dictionary<string, string>(2);

            result["app"] = AppDomain.CurrentDomain.FriendlyName;

            if (string.IsNullOrWhiteSpace(_options.AccessKey)
                && string.IsNullOrWhiteSpace(_options.SecretKey))
                return result;

            string signData = string.IsNullOrWhiteSpace(serviceName)
                ? DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "@@" + serviceName
                : DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

            string signature = Utilities.HashUtil.GetHMACSHA1(signData, _options.SecretKey);
            result["signature"] = signature;
            result["data"] = signData;
            result["ak"] = _options.AccessKey;

            return result;
        }
    }
}
