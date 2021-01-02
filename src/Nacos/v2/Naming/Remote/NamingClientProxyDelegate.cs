namespace Nacos.V2.Naming.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Core;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Remote.Grpc;
    using Nacos.V2.Naming.Remote.Http;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Remote;

    public class NamingClientProxyDelegate : INamingClientProxy, IDisposable
    {
        private ServerListManager serverListManager;

        /*private  ServiceInfoUpdateService serviceInfoUpdateService;*/

        private ServiceInfoHolder serviceInfoHolder;

        /*private NamingHttpClientProxy httpClientProxy;*/

        private NamingGrpcClientProxy grpcClientProxy;

        public NamingClientProxyDelegate(ILogger logger, string @namespace, ServiceInfoHolder serviceInfoHolder, IOptionsMonitor<NacosOptions> options)
        {
            this.serverListManager = new ServerListManager(logger, options.CurrentValue);
            this.serviceInfoHolder = serviceInfoHolder;
            this.grpcClientProxy = new NamingGrpcClientProxy(logger, @namespace, serverListManager, options, serviceInfoHolder);
        }

        public Task CreateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        public Task<bool> DeleteService(string serviceName, string groupName) => Task.FromResult(false);

        public async Task DeregisterService(string serviceName, string groupName, Instance instance)
            => await GetExecuteClientProxy().DeregisterService(serviceName, groupName, instance);

        public void Dispose()
        {
            grpcClientProxy.Dispose();
        }

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

            // serviceInfoUpdateService.scheduleUpdateIfAbsent(serviceName, groupName, clusters);
            serviceInfoHolder.ProcessServiceInfo(result);
            return result;
        }

        public async Task Unsubscribe(string serviceName, string groupName, string clusters)
            => await GetExecuteClientProxy().Unsubscribe(serviceName, groupName, clusters);

        public Task UpdateBeatInfo(List<Instance> modifiedInstances)
        {
            // HTTP
            return Task.CompletedTask;
        }

        public Task UpdateInstance(string serviceName, string groupName, Instance instance) => Task.CompletedTask;

        public Task UpdateService(Service service, AbstractSelector selector) => Task.CompletedTask;

        private INamingClientProxy GetExecuteClientProxy()
        {
            return grpcClientProxy;
        }
    }
}
