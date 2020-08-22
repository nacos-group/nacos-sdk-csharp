namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Nacos.Utilities;
    using System;
    using System.Threading.Tasks;

    public class HostReactor
    {
        private readonly ILogger _logger;
        private readonly NacosOptions _options;

        public bool Flag { get; set; } = false;

        public EventDispatcher _eventDispatcher;

        public IDictionary<string, ServiceInfo> ServiceInfoMap { get; set; } = new ConcurrentDictionary<string, ServiceInfo>();

        public HostReactor(
            ILoggerFactory loggerFactory,
            NacosOptions optionAccs,
            EventDispatcher eventDispatcher)
        {
            _logger = loggerFactory.CreateLogger<NacosNamingClient>();
            _options = optionAccs;
            _eventDispatcher = eventDispatcher;
        }

        private ServiceInfo GetServiceInfo0(String serviceName, String clusters)
        {
            String key = ServiceInfo.getKey(serviceName, clusters);
            ServiceInfo serviceObj = null;
            ServiceInfoMap.TryGetValue(key, out serviceObj);
            return serviceObj;
        }

        public ServiceInfo GetServiceInfo(string serviceName, string clusters)
        {
            string key = ServiceInfo.getKey(serviceName, clusters);

            ServiceInfo serviceObj = GetServiceInfo0(serviceName, clusters);

            if (serviceObj == null)
            {
                serviceObj = new ServiceInfo(serviceName, clusters);

                ServiceInfoMap[serviceObj.getKey()] = serviceObj;
            }

            return serviceObj;
        }

        public Task ProcessServiceJson(string result)
        {
            var obj = result.ToObj<ServiceInfo>();
            ServiceInfo newService = obj;
            ServiceInfo oldService = null;
            ServiceInfoMap.TryGetValue(obj.getKey(), out oldService);
            if (obj.Hosts == null || !newService.validate())
            {
                Flag = false;
                return Task.CompletedTask;
            }

            if (oldService != null)
            {
                // Updating the new service into the Map
                ServiceInfoMap[obj.getKey()] = obj;
                IDictionary<string, Host> oldHostMap = new ConcurrentDictionary<string, Host>();
                foreach (var entry in oldService.Hosts)
                {
                    oldHostMap[entry.ToInetAddr()] = entry;
                }

                IDictionary<string, Host> newHostMap = new ConcurrentDictionary<string, Host>();
                foreach (var entry in newService.Hosts)
                {
                    newHostMap[entry.ToInetAddr()] = entry;
                }

                foreach (KeyValuePair<string, Host> entry in newHostMap)
                {
                    if (!oldHostMap.ContainsKey(entry.Key))
                    {
                        _eventDispatcher.ServiceChanged(obj);
                        Flag = true;
                    }
                    else
                    {
                        Host host1 = newHostMap[entry.Key];
                        Host host2 = oldHostMap[entry.Key];
                        if (host1.ToString() == host2.ToString())
                        {
                            Flag = false;
                        }
                        else
                        {
                            _eventDispatcher.ServiceChanged(obj);
                            Flag = true;
                            return Task.CompletedTask;
                        }
                    }
                }

                foreach (KeyValuePair<string, Host> entry in oldHostMap)
                {
                    if (!newHostMap.ContainsKey(entry.Key))
                    {
                        _eventDispatcher.ServiceChanged(obj);
                        Flag = true;
                        return Task.CompletedTask;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
                ServiceInfoMap[obj.getKey()] = obj;
                _eventDispatcher.ServiceChanged(obj);
                Flag = true;
            }

            return Task.CompletedTask;
        }
    }
}