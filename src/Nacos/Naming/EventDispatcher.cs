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
        private volatile bool closed = false;
        public readonly BlockingCollection<ServiceInfo> ChangedServices = new BlockingCollection<ServiceInfo>(boundedCapacity: 10);

        public ConcurrentDictionary<string, List<Listener>> ObserverMap { get; set; } = new ConcurrentDictionary<string, List<Listener>>();

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
                    List<Listener> listeners = ObserverMap[serviceInfo.getKey()];
                    File.AppendAllText("output2.txt", "Listeners Count" + listeners.Count + System.Environment.NewLine);
                    if (listeners.Count != 0)
                    {
                        foreach (Listener listener in listeners)
                        {
                            List<Host> hosts = serviceInfo.Hosts;
                            File.AppendAllText("output2.txt", "Miracle happened :)" + System.Environment.NewLine);

                            // listener.onEvent(new NamingEvent(serviceInfo.getName(), serviceInfo.getGroupName(),
                            //     serviceInfo.getClusters(), hosts));
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning("[NA] notify error for service: " + serviceInfo.name + ", clusters: " + serviceInfo.clusters, e);
                }
            }
        }

        public void ServiceChanged(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                return;
            }

            ChangedServices.Add(serviceInfo);
        }
    }
}