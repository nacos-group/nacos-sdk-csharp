namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Nacos.Exceptions;
    using Nacos.Utilities;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class HostReactor
    {
        private readonly ILogger _logger;
        private readonly NacosOptions _options;

        public IDictionary<string, ServiceInfo> ServiceInfoMap { get; } = new ConcurrentDictionary<string, ServiceInfo>();

        public HostReactor(
            ILoggerFactory loggerFactory,
            NacosOptions optionAccs)
        {
            _logger = loggerFactory.CreateLogger<NacosNamingClient>();
            _options = optionAccs;
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
    }
}