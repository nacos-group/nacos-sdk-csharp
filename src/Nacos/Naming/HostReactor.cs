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
            File.AppendAllText("output.txt", "Old Host Id:" + " " + oldService.ToJsonString() + System.Environment.NewLine);
            if (obj.Hosts == null)
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
                    File.AppendAllText("output.txt", "Old Host Id:" + " " + entry.InstanceId + "Host Weight" + entry.Weight + System.Environment.NewLine);
                }

                IDictionary<string, Host> newHostMap = new ConcurrentDictionary<string, Host>();
                foreach (var entry in newService.Hosts)
                {
                    newHostMap[entry.ToInetAddr()] = entry;
                    File.AppendAllText("output.txt", "New Host Id:" + " " + entry.InstanceId + "Host weight" + entry.Weight + System.Environment.NewLine);
                }

                foreach (KeyValuePair<string, Host> entry in newHostMap)
                {
                    if (!oldHostMap.ContainsKey(entry.Key))
                    {
                        _eventDispatcher.ServiceChanged(obj);
                        File.AppendAllText("output.txt", "Service Changed" + System.Environment.NewLine);
                        Flag = true;
                    }
                    else
                    {
                        Host host1 = newHostMap[entry.Key];
                        Host host2 = oldHostMap[entry.Key];
                        File.AppendAllText("output1.txt", "New Host Id:" + " " + host1.String() + System.Environment.NewLine);
                        File.AppendAllText("output1.txt", "Old Host Id:" + " " + host2.String() + System.Environment.NewLine);
                        if (host1.String() == host2.String())
                        {
                            Flag = false;
                        }
                        else
                        {
                            _eventDispatcher.ServiceChanged(obj);
                            File.AppendAllText("output.txt", "Service Changed" + System.Environment.NewLine);
                            Flag = true;
                            return Task.CompletedTask;
                        }
                    }
                }

                foreach (KeyValuePair<string, Host> entry in oldHostMap)
                {
                    if (!newHostMap.ContainsKey(entry.Key))
                    {
                        File.AppendAllText("output.txt", "Service Changed" + System.Environment.NewLine);
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
                File.AppendAllText("output.txt", "Service Subscribe Called" + obj.ToJsonString() + System.Environment.NewLine);
                Flag = true;
            }

            return Task.CompletedTask;
        }
    }
}