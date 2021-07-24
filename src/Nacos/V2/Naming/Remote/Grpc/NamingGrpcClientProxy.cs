namespace Nacos.V2.Naming.Remote.Grpc
{
    using Microsoft.Extensions.Logging;
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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class NamingGrpcClientProxy : INamingClientProxy, IDisposable
    {
        private readonly ILogger _logger;

        private string namespaceId;

        private string uuid;

        private long requestTimeout;

        private RpcClient rpcClient;

        private SecurityProxy _securityProxy;

        private NacosSdkOptions _options;

        private NamingGrpcRedoService _redoService;

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

            this.requestTimeout = options.DefaultTimeOut > 0 ? options.DefaultTimeOut : 3000L;

            Dictionary<string, string> labels = new Dictionary<string, string>()
            {
                { RemoteConstants.LABEL_SOURCE, RemoteConstants.LABEL_SOURCE_SDK },
                { RemoteConstants.LABEL_MODULE, RemoteConstants.LABEL_MODULE_NAMING },
            };

            this.rpcClient = RpcClientFactory.CreateClient(uuid, RemoteConnectionType.GRPC, labels);
            this._redoService = new NamingGrpcRedoService(_logger, this);

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

        public async Task DoRegisterService(string serviceName, string groupName, Instance instance)
        {
            var request = new InstanceRequest(namespaceId, serviceName, groupName, NamingRemoteConstants.REGISTER_INSTANCE, instance);
            await RequestToServer<CommonResponse>(request).ConfigureAwait(false);
            _redoService.InstanceRegistered(serviceName, groupName);
        }

        public bool ServerHealthy() => rpcClient.IsRunning();

        public async Task<ServiceInfo> Subscribe(string serviceName, string groupName, string clusters)
        {
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
                request.PutAllHeader(GetSecurityHeaders());
                request.PutAllHeader(GetSpasHeaders(NamingUtils.GetGroupedNameOptional(request.ServiceName, request.GroupName)));

                CommonResponse response =
                        requestTimeout < 0
                        ? await rpcClient.Request(request)
.ConfigureAwait(false) : await rpcClient.Request(request, requestTimeout).ConfigureAwait(false);

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

            result[CommonParams.APP_FILED] = AppDomain.CurrentDomain.FriendlyName;

            if (string.IsNullOrWhiteSpace(_options.AccessKey)
                && string.IsNullOrWhiteSpace(_options.SecretKey))
                return result;

            string signData = !string.IsNullOrWhiteSpace(serviceName)
                ? DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + CommonParams.SEPARATOR + serviceName
                : DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

            string signature = HashUtil.GetHMACSHA1(signData, _options.SecretKey);
            result[CommonParams.SIGNATURE_FILED] = signature;
            result[CommonParams.DATA_FILED] = signData;
            result[CommonParams.AK_FILED] = _options.AccessKey;

            return result;
        }
    }
}
