namespace Nacos.V2.Naming
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Common;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Core;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Event;
    using Nacos.V2.Naming.Remote;
    using Nacos.V2.Remote;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class NacosNamingService : INacosNamingService
    {
        private readonly ILogger _logger;
        private readonly NacosSdkOptions _options;

        private string _namespace;

        private ServiceInfoHolder _serviceInfoHolder;

        private InstancesChangeNotifier _changeNotifier;

        private INamingClientProxy _clientProxy;

        public NacosNamingService(
            ILoggerFactory loggerFactory,
            IOptionsMonitor<NacosSdkOptions> optionAccs)
        {
            _logger = loggerFactory.CreateLogger<NacosNamingService>();
            _options = optionAccs.CurrentValue;
            _namespace = _options.Namespace;
            this._changeNotifier = new InstancesChangeNotifier();
            this._serviceInfoHolder = new ServiceInfoHolder(_logger, _namespace, _options, _changeNotifier);
            this._clientProxy = new NamingClientProxyDelegate(_logger, _namespace, _serviceInfoHolder, optionAccs, _changeNotifier);
        }

        public async Task DeregisterInstance(string serviceName, string ip, int port)
            => await DeregisterInstance(serviceName, ip, port, Constants.DEFAULT_CLUSTER_NAME);

        public async Task DeregisterInstance(string serviceName, string groupName, string ip, int port)
            => await DeregisterInstance(serviceName, groupName, ip, port, Constants.DEFAULT_CLUSTER_NAME);

        public async Task DeregisterInstance(string serviceName, string ip, int port, string clusterName)
            => await DeregisterInstance(serviceName, Constants.DEFAULT_GROUP, ip, port, clusterName);

        public async Task DeregisterInstance(string serviceName, string groupName, string ip, int port, string clusterName)
        {
            var instance = new Instance()
            {
                Ip = ip,
                Port = port,
                ClusterName = clusterName
            };

            await DeregisterInstance(serviceName, groupName, instance);
        }

        public async Task DeregisterInstance(string serviceName, Instance instance)
            => await DeregisterInstance(serviceName, Constants.DEFAULT_GROUP, instance);

        public async Task DeregisterInstance(string serviceName, string groupName, Instance instance)
            => await _clientProxy.DeregisterService(serviceName, groupName, instance);

        public async Task<List<Instance>> GetAllInstances(string serviceName)
            => await GetAllInstances(serviceName, new List<string>());

        public async Task<List<Instance>> GetAllInstances(string serviceName, string groupName)
            => await GetAllInstances(serviceName, groupName, new List<string>());

        public async Task<List<Instance>> GetAllInstances(string serviceName, bool subscribe)
            => await GetAllInstances(serviceName, new List<string>(), subscribe);

        public async Task<List<Instance>> GetAllInstances(string serviceName, string groupName, bool subscribe)
            => await GetAllInstances(serviceName, groupName, new List<string>(), subscribe);

        public async Task<List<Instance>> GetAllInstances(string serviceName, List<string> clusters)
            => await GetAllInstances(serviceName, Constants.DEFAULT_GROUP, clusters, true);

        public async Task<List<Instance>> GetAllInstances(string serviceName, string groupName, List<string> clusters)
             => await GetAllInstances(serviceName, groupName, clusters, true);

        public async Task<List<Instance>> GetAllInstances(string serviceName, List<string> clusters, bool subscribe)
            => await GetAllInstances(serviceName, Constants.DEFAULT_GROUP, clusters, subscribe);

        public async Task<List<Instance>> GetAllInstances(string serviceName, string groupName, List<string> clusters, bool subscribe)
        {
            ServiceInfo serviceInfo;
            string clusterString = string.Join(",", clusters);
            if (subscribe)
            {
                serviceInfo = _serviceInfoHolder.GetServiceInfo(serviceName, groupName, clusterString);
                if (serviceInfo == null)
                {
                    serviceInfo = await _clientProxy.Subscribe(serviceName, groupName, clusterString);
                }
            }
            else
            {
                serviceInfo = await _clientProxy.QueryInstancesOfService(serviceName, groupName, clusterString, 0, false);
            }

            List<Instance> list = serviceInfo.Hosts;
            if (serviceInfo == null || serviceInfo.Hosts == null || !serviceInfo.Hosts.Any())
            {
                return new List<Instance>();
            }

            return list;
        }

        public Task<string> GetServerStatus()
            => Task.FromResult(_clientProxy.ServerHealthy() ? "UP" : "DOWN");

        public async Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize)
            => await GetServicesOfServer(pageNo, pageSize, Constants.DEFAULT_GROUP);

        public async Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, string groupName)
            => await GetServicesOfServer(pageNo, pageSize, groupName, null);

        public async Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, AbstractSelector selector)
            => await GetServicesOfServer(pageNo, pageSize, Constants.DEFAULT_GROUP, selector);

        public async Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, string groupName, AbstractSelector selector)
            => await _clientProxy.GetServiceList(pageNo, pageSize, groupName, selector);

        public Task<List<ServiceInfo>> GetSubscribeServices()
            => Task.FromResult(_changeNotifier.GetSubscribeServices());

        public async Task RegisterInstance(string serviceName, string ip, int port)
            => await RegisterInstance(serviceName, ip, port, Constants.DEFAULT_CLUSTER_NAME);

        public async Task RegisterInstance(string serviceName, string groupName, string ip, int port)
            => await RegisterInstance(serviceName, groupName, ip, port, Constants.DEFAULT_CLUSTER_NAME);

        public async Task RegisterInstance(string serviceName, string ip, int port, string clusterName)
            => await RegisterInstance(serviceName, Constants.DEFAULT_GROUP, ip, port, clusterName);

        public async Task RegisterInstance(string serviceName, string groupName, string ip, int port, string clusterName)
        {
            var instance = new Instance()
            {
                Ip = ip,
                Port = port,
                Weight = 1.0d,
                ClusterName = clusterName
            };

            await RegisterInstance(serviceName, groupName, instance);
        }

        public async Task RegisterInstance(string serviceName, Instance instance)
            => await RegisterInstance(serviceName, Constants.DEFAULT_GROUP, instance);

        public async Task RegisterInstance(string serviceName, string groupName, Instance instance)
            => await _clientProxy.RegisterServiceAsync(serviceName, groupName, instance);

        public async Task<List<Instance>> SelectInstances(string serviceName, bool healthy)
            => await SelectInstances(serviceName, new List<string>(), healthy);

        public async Task<List<Instance>> SelectInstances(string serviceName, string groupName, bool healthy)
            => await SelectInstances(serviceName, groupName, healthy, true);

        public async Task<List<Instance>> SelectInstances(string serviceName, bool healthy, bool subscribe)
            => await SelectInstances(serviceName, new List<string>(), healthy, subscribe);

        public async Task<List<Instance>> SelectInstances(string serviceName, string groupName, bool healthy, bool subscribe)
            => await SelectInstances(serviceName, groupName, new List<string>(), healthy, subscribe);

        public async Task<List<Instance>> SelectInstances(string serviceName, List<string> clusters, bool healthy)
            => await SelectInstances(serviceName, clusters, healthy, true);

        public async Task<List<Instance>> SelectInstances(string serviceName, string groupName, List<string> clusters, bool healthy)
            => await SelectInstances(serviceName, groupName, clusters, healthy, true);

        public async Task<List<Instance>> SelectInstances(string serviceName, List<string> clusters, bool healthy, bool subscribe)
            => await SelectInstances(serviceName, Constants.DEFAULT_GROUP, clusters, healthy, subscribe);

        public async Task<List<Instance>> SelectInstances(string serviceName, string groupName, List<string> clusters, bool healthy, bool subscribe)
        {
            ServiceInfo serviceInfo;
            string clusterString = string.Join(",", clusters);
            if (subscribe)
            {
                serviceInfo = _serviceInfoHolder.GetServiceInfo(serviceName, groupName, clusterString);
                if (serviceInfo == null)
                {
                    serviceInfo = await _clientProxy.Subscribe(serviceName, groupName, clusterString);
                }
            }
            else
            {
                serviceInfo = await _clientProxy.QueryInstancesOfService(serviceName, groupName, clusterString, 0, false);
            }

            return SelectInstances(serviceInfo, healthy);
        }

        private List<Instance> SelectInstances(ServiceInfo serviceInfo, bool healthy)
        {
            List<Instance> list = serviceInfo.Hosts;

            if (serviceInfo == null || list == null || !list.Any()) return new List<Instance>();

            return list.Where(x => x.Healthy.Equals(healthy) && x.Enabled && x.Weight > 0).ToList();
        }

        public async Task<Instance> SelectOneHealthyInstance(string serviceName)
            => await SelectOneHealthyInstance(serviceName, new List<string>());

        public async Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName)
            => await SelectOneHealthyInstance(serviceName, groupName, true);

        public async Task<Instance> SelectOneHealthyInstance(string serviceName, bool subscribe)
            => await SelectOneHealthyInstance(serviceName, new List<string>(), subscribe);

        public async Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, bool subscribe)
            => await SelectOneHealthyInstance(serviceName, groupName, new List<string>(), subscribe);

        public async Task<Instance> SelectOneHealthyInstance(string serviceName, List<string> clusters)
            => await SelectOneHealthyInstance(serviceName, clusters, true);

        public async Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, List<string> clusters)
            => await SelectOneHealthyInstance(serviceName, groupName, clusters, true);

        public async Task<Instance> SelectOneHealthyInstance(string serviceName, List<string> clusters, bool subscribe)
            => await SelectOneHealthyInstance(serviceName, Constants.DEFAULT_GROUP, clusters, subscribe);

        public async Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, List<string> clusters, bool subscribe)
        {
            string clusterString = string.Join(",", clusters);
            if (subscribe)
            {
                ServiceInfo serviceInfo = _serviceInfoHolder.GetServiceInfo(serviceName, groupName, clusterString);

                if (serviceInfo == null)
                {
                    serviceInfo = await _clientProxy.Subscribe(serviceName, groupName, clusterString);
                }

                return Balancer.GetHostByRandom(serviceInfo?.Hosts);
            }
            else
            {
                ServiceInfo serviceInfo = await _clientProxy
                        .QueryInstancesOfService(serviceName, groupName, clusterString, 0, false);

                return Balancer.GetHostByRandom(serviceInfo?.Hosts);
            }
        }

        public Task ShutDown() => Task.CompletedTask;

        public async Task Subscribe(string serviceName, IEventListener listener)
            => await Subscribe(serviceName, new List<string>(), listener);

        public async Task Subscribe(string serviceName, string groupName, IEventListener listener)
            => await Subscribe(serviceName, groupName, new List<string>(), listener);

        public async Task Subscribe(string serviceName, List<string> clusters, IEventListener listener)
            => await Subscribe(serviceName, Constants.DEFAULT_GROUP, clusters, listener);

        public async Task Subscribe(string serviceName, string groupName, List<string> clusters, IEventListener listener)
        {
            if (listener == null) return;

            string clusterString = string.Join(",", clusters);

            _changeNotifier.RegisterListener(groupName, serviceName, clusterString, listener);
            await _clientProxy.Subscribe(serviceName, groupName, clusterString);
        }

        public async Task Unsubscribe(string serviceName, IEventListener listener)
            => await Unsubscribe(serviceName, new List<string>(), listener);

        public async Task Unsubscribe(string serviceName, string groupName, IEventListener listener)
            => await Unsubscribe(serviceName, groupName, new List<string>(), listener);

        public async Task Unsubscribe(string serviceName, List<string> clusters, IEventListener listener)
            => await Unsubscribe(serviceName, Constants.DEFAULT_GROUP, clusters, listener);

        public async Task Unsubscribe(string serviceName, string groupName, List<string> clusters, IEventListener listener)
        {
            string clustersString = string.Join(",", clusters);

            _changeNotifier.DeregisterListener(groupName, serviceName, clustersString, listener);
            if (!_changeNotifier.IsSubscribed(groupName, serviceName, clustersString))
            {
                await _clientProxy.Unsubscribe(serviceName, groupName, clustersString);
            }
        }
    }
}
