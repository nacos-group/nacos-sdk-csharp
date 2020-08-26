namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class EventDispatcher
    {
        private readonly ILogger _logger;
        private bool closed = false;

        private readonly BlockingCollection<ServiceInfo> _changedServices = new BlockingCollection<ServiceInfo>(boundedCapacity: 10);
        private readonly ConcurrentDictionary<string, List<Action<IEvent>>> _observerMap = new ConcurrentDictionary<string, List<Action<IEvent>>>();

        public EventDispatcher(
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<NacosNamingClient>();
            Task.Factory.StartNew(() => EventListener());
        }

        private void EventListener()
        {
            while (!closed)
            {
                if (_changedServices.TryTake(out var serviceInfo, 5000))
                {
                    try
                    {
                        if (_observerMap.TryGetValue(serviceInfo.GetKey(), out var actions))
                        {
                            if (actions != null && actions.Any())
                            {
                                foreach (Action<IEvent> action in actions)
                                {
                                    List<Host> hosts = serviceInfo.Hosts;
                                    action.Invoke(new NamingEvent(serviceInfo.name, serviceInfo.groupName, serviceInfo.clusters, hosts));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "[NA] notify error for service: {0}, clusters: {1}", serviceInfo.name, serviceInfo.clusters);
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        public void AddListener(ServiceInfo serviceInfo, string clusters, Action<IEvent> listener)
        {
            _logger.LogInformation("[LISTENER] adding {0} with {1} to listener map", serviceInfo.name, clusters);

            var observers = new List<Action<IEvent>>
            {
                listener
            };

            _observerMap.AddOrUpdate(ServiceInfo.GetKey(serviceInfo.name, clusters), observers, (k, v) =>
            {
                v.Add(listener);
                return v;
            });
        }

        public void RemoveListener(string serviceName, string clusters, Action<IEvent> listener)
        {
            _logger.LogInformation("[LISTENER] removing {0} with {clusters} from listener map", serviceName, clusters);

            if (_observerMap.TryGetValue(ServiceInfo.GetKey(serviceName, clusters), out var observers))
            {
                observers.Remove(listener);
                if (observers.Count <= 0)
                {
                    _observerMap.TryRemove(ServiceInfo.GetKey(serviceName, clusters), out _);
                }
            }
        }

        public void ServiceChanged(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null) return;

            _changedServices.Add(serviceInfo);
        }
    }
}