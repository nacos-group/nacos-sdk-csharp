namespace Nacos.Naming.Remote
{
    using Microsoft.Extensions.Logging;
    using Nacos;
    using Nacos.Naming.Cache;
    using Nacos.Naming.Core;
    using Nacos.Naming.Dtos;
    using Nacos.Naming.Event;
    using Nacos.Naming.Remote.Grpc;
    using Nacos.Naming.Remote.Http;
    using Nacos.Naming.Utils;
    using Nacos.Remote;
    using Nacos.Security;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
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

        public NamingClientProxyDelegate(ILogger logger, string @namespace, ServiceInfoHolder serviceInfoHolder, NacosSdkOptions options, InstancesChangeNotifier changeNotifier, IHttpClientFactory clientFactory)
        {
            _options = options;
            serverListManager = new ServerListManager(logger, options, @namespace);
            this.serviceInfoHolder = serviceInfoHolder;
            securityProxy = null; /* new SecurityProxy(options, logger); */
            InitSecurityProxy();
            _serviceInfoUpdateService = new ServiceInfoUpdateService(logger, options, serviceInfoHolder, this, changeNotifier);

            grpcClientProxy = new NamingGrpcClientProxy(logger, @namespace, securityProxy, serverListManager, options, serviceInfoHolder);
            httpClientProxy = new NamingHttpClientProxy(logger, @namespace, securityProxy, serverListManager, options, serviceInfoHolder, clientFactory);
        }

        private void InitSecurityProxy()
        {
            _loginTimer = new Timer(
                async x =>
                {
                    await securityProxy.LoginAsync(serverListManager.GetServerList()).ConfigureAwait(false);
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_securityInfoRefreshIntervalMills));

            // init should wait the result.
            securityProxy.LoginAsync(serverListManager.GetServerList()).Wait();
        }

        public Task CreateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        public Task<bool> DeleteService(string serviceName, string groupName) => Task.FromResult(false);

        public async Task DeregisterService(string serviceName, string groupName, Instance instance)
            => await GetExecuteClientProxy(instance).DeregisterService(serviceName, groupName, instance).ConfigureAwait(false);

        public void Dispose() => grpcClientProxy?.Dispose();

        public async Task<ListView<string>> GetServiceList(int pageNo, int pageSize, string groupName, AbstractSelector selector)
            => await grpcClientProxy.GetServiceList(pageNo, pageSize, groupName, selector).ConfigureAwait(false);

        public async Task<ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, int udpPort, bool healthyOnly)
            => await grpcClientProxy.QueryInstancesOfService(serviceName, groupName, clusters, udpPort, healthyOnly).ConfigureAwait(false);

        public Task<Service> QueryService(string serviceName, string groupName) => Task.FromResult<Service>(null);

        public async Task RegisterServiceAsync(string serviceName, string groupName, Instance instance)
            => await GetExecuteClientProxy(instance).RegisterServiceAsync(serviceName, groupName, instance).ConfigureAwait(false);

        public bool ServerHealthy() => grpcClientProxy?.ServerHealthy() ?? httpClientProxy?.ServerHealthy() ?? false;

        public async Task<ServiceInfo> Subscribe(string serviceName, string groupName, string clusters)
        {
            string serviceNameWithGroup = NamingUtils.GetGroupedName(serviceName, groupName);
            string serviceKey = ServiceInfo.GetKey(serviceNameWithGroup, clusters);

            if (!serviceInfoHolder.GetServiceInfoMap().TryGetValue(serviceKey, out var result)
                || !await IsSubscribed(serviceName, groupName, clusters).ConfigureAwait(false))
            {
                result = await grpcClientProxy.Subscribe(serviceName, groupName, clusters).ConfigureAwait(false);
            }

            _serviceInfoUpdateService.ScheduleUpdateIfAbsent(serviceName, groupName, clusters);
            serviceInfoHolder.ProcessServiceInfo(result);
            return result;
        }

        public async Task Unsubscribe(string serviceName, string groupName, string clusters)
        {
            _serviceInfoUpdateService.StopUpdateIfContain(serviceName, groupName, clusters);
            await grpcClientProxy.Unsubscribe(serviceName, groupName, clusters).ConfigureAwait(false);
        }

        public async Task UpdateBeatInfo(List<Instance> modifiedInstances)
            => await httpClientProxy.UpdateBeatInfo(modifiedInstances).ConfigureAwait(false);

        public Task UpdateInstance(string serviceName, string groupName, Instance instance) => Task.CompletedTask;

        public Task UpdateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        private INamingClientProxy GetExecuteClientProxy(Instance instance) => instance.Ephemeral ? grpcClientProxy : httpClientProxy;

        public Task<bool> IsSubscribed(string serviceName, string groupName, string clusters)
            => grpcClientProxy.IsSubscribed(serviceName, groupName, clusters);

        public async Task BatchRegisterServiceAsync(string serviceName, string groupName, List<Instance> instances)
        {
            if (instances == null || !instances.Any()) await Task.Yield();

            await grpcClientProxy.BatchRegisterServiceAsync(serviceName, groupName, instances).ConfigureAwait(false);
        }
    }
}
