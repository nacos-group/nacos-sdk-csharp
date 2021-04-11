namespace Nacos.V2.Naming.Remote
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Core;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Event;
    using Nacos.V2.Naming.Remote.Grpc;
    using Nacos.V2.Naming.Remote.Http;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Remote;
    using Nacos.V2.Security;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class NamingClientProxyDelegate : INamingClientProxy, IDisposable
    {
        private NacosSdkOptions _options;

        private ServerListManager serverListManager;

        private ServiceInfoUpdateService _serviceInfoUpdateService;

        private ServiceInfoHolder serviceInfoHolder;

        private NamingHttpClientProxy httpClientProxy;

        private NamingGrpcClientProxy grpcClientProxy;

        private SecurityProxy securityProxy;

        private Timer _loginTimer;

        private long _securityInfoRefreshIntervalMills = 5000;

        public NamingClientProxyDelegate(ILogger logger, string @namespace, ServiceInfoHolder serviceInfoHolder, NacosSdkOptions options, InstancesChangeNotifier changeNotifier)
        {
            this._options = options;
            this.serverListManager = new ServerListManager(logger, options);
            this.serviceInfoHolder = serviceInfoHolder;
            this.securityProxy = new SecurityProxy(options, logger);
            InitSecurityProxy();
            this._serviceInfoUpdateService = new ServiceInfoUpdateService(logger, options, serviceInfoHolder, this, changeNotifier);
            this.grpcClientProxy = new NamingGrpcClientProxy(logger, @namespace, securityProxy, serverListManager, options, serviceInfoHolder);
            this.httpClientProxy = new NamingHttpClientProxy(logger, @namespace, securityProxy, serverListManager, options, serviceInfoHolder);
        }

        private void InitSecurityProxy()
        {
            _loginTimer = new Timer(
                async x =>
                {
                    await securityProxy.LoginAsync(serverListManager.GetServerList());
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_securityInfoRefreshIntervalMills));

            // init should wait the result.
            securityProxy.LoginAsync(serverListManager.GetServerList()).Wait();
        }

        public Task CreateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        public Task<bool> DeleteService(string serviceName, string groupName) => Task.FromResult(false);

        public async Task DeregisterService(string serviceName, string groupName, Instance instance)
            => await GetExecuteClientProxy().DeregisterService(serviceName, groupName, instance);

        public void Dispose() => grpcClientProxy.Dispose();

        public async Task<ListView<string>> GetServiceList(int pageNo, int pageSize, string groupName, AbstractSelector selector)
            => await GetExecuteClientProxy().GetServiceList(pageNo, pageSize, groupName, selector);

        public async Task<ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, int udpPort, bool healthyOnly)
            => await GetExecuteClientProxy().QueryInstancesOfService(serviceName, groupName, clusters, udpPort, healthyOnly);

        public Task<Service> QueryService(string serviceName, string groupName) => Task.FromResult<Service>(null);

        public async Task RegisterServiceAsync(string serviceName, string groupName, Instance instance)
            => await GetExecuteClientProxy().RegisterServiceAsync(serviceName, groupName, instance);

        public bool ServerHealthy() => grpcClientProxy.ServerHealthy();

        public async Task<ServiceInfo> Subscribe(string serviceName, string groupName, string clusters)
        {
            string serviceNameWithGroup = NamingUtils.GetGroupedName(serviceName, groupName);
            string serviceKey = ServiceInfo.GetKey(serviceNameWithGroup, clusters);

            if (!serviceInfoHolder.GetServiceInfoMap().TryGetValue(serviceKey, out var result))
            {
                result = await GetExecuteClientProxy().Subscribe(serviceName, groupName, clusters);
            }

            _serviceInfoUpdateService.ScheduleUpdateIfAbsent(serviceName, groupName, clusters);
            serviceInfoHolder.ProcessServiceInfo(result);
            return result;
        }

        public async Task Unsubscribe(string serviceName, string groupName, string clusters)
        {
            _serviceInfoUpdateService.StopUpdateIfContain(serviceName, groupName, clusters);
            await GetExecuteClientProxy().Unsubscribe(serviceName, groupName, clusters);
        }

        public async Task UpdateBeatInfo(List<Instance> modifiedInstances)
            => await httpClientProxy.UpdateBeatInfo(modifiedInstances);

        public Task UpdateInstance(string serviceName, string groupName, Instance instance) => Task.CompletedTask;

        public Task UpdateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        private INamingClientProxy GetExecuteClientProxy() => _options.NamingUseRpc ? grpcClientProxy : httpClientProxy;
    }
}
