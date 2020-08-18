namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Nacos.Exceptions;
    using Nacos.Utilities;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class EventDispatcher
    {
        private readonly ILogger _logger;
        private readonly NacosOptions _options;
        private bool closed = false;
        public readonly BlockingCollection<ServiceInfo> ChangedServices = new BlockingCollection<ServiceInfo>(boundedCapacity: 10);

        public ConcurrentDictionary<string, List<Action<IEvent>>> ObserverMap { get; set; } = new ConcurrentDictionary<string, List<Action<IEvent>>>();

        public EventDispatcher(
            ILoggerFactory loggerFactory,
            NacosOptions optionAccs)
        {
            _logger = loggerFactory.CreateLogger<NacosNamingClient>();
            _options = optionAccs;
            Task.Factory.StartNew(() => EventListener());
        }

        public void EventListener()
        {
            while (!closed)
            {
                ServiceInfo serviceInfo = null;
                serviceInfo = ChangedServices.Take();
                if (serviceInfo == null)
                {
                    continue;
                }

                try
                {
                    File.AppendAllText("output3.txt", "Map Value" + ObserverMap[serviceInfo.getKey()].ToJsonString() + System.Environment.NewLine);
                    List<Action<IEvent>> actions = ObserverMap[serviceInfo.getKey()];
                    if (actions.Count != 0)
                    {
                        foreach (Action<IEvent> action in actions)
                        {
                            List<Host> hosts = serviceInfo.Hosts;
                            File.AppendAllText("output2.txt", "Miracle happened :)" + System.Environment.NewLine);
                            action.Invoke(new NamingEvent(serviceInfo.name, serviceInfo.groupName, serviceInfo.clusters, hosts));
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "[NA] notify error for service: {0}, clusters: {1}",  serviceInfo.name, serviceInfo.clusters);
                }
            }
        }

        public void ServiceChanged(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                return;
            }

            File.AppendAllText("output2.txt", "Observer Count" + ObserverMap.Count + System.Environment.NewLine);
            ChangedServices.Add(serviceInfo);
        }
    }
}