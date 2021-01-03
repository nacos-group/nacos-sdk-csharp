namespace Nacos.V2.Naming.Cache
{
    using Microsoft.Extensions.Logging;
    using Nacos.Utilities;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Naming.Backups;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    public class ServiceInfoHolder
    {
        private ConcurrentDictionary<string, Dtos.ServiceInfo> serviceInfoMap;

        private FailoverReactor failoverReactor;

        private string cacheDir = "";

        private ILogger _logger;

        public ServiceInfoHolder(ILogger logger, string @namespace, NacosOptions nacosOptions)
        {
            this._logger = logger;

            InitCacheDir(@namespace);

            if (IsLoadCacheAtStart(nacosOptions))
            {
                var data = DiskCache.ReadAsync(this.cacheDir).ConfigureAwait(false).GetAwaiter().GetResult();
                this.serviceInfoMap = new ConcurrentDictionary<string, Dtos.ServiceInfo>(data);
            }
            else
            {
                this.serviceInfoMap = new ConcurrentDictionary<string, Dtos.ServiceInfo>();
            }

            this.failoverReactor = new FailoverReactor(this, cacheDir);
        }

        private bool IsLoadCacheAtStart(NacosOptions nacosOptions)
        {
            bool loadCacheAtStart = false;
            if (nacosOptions != null && string.IsNullOrWhiteSpace(nacosOptions.NamingLoadCacheAtStart))
            {
                loadCacheAtStart = Convert.ToBoolean(nacosOptions.NamingLoadCacheAtStart);
            }

            return loadCacheAtStart;
        }

        internal Dtos.ServiceInfo ProcessServiceInfo(Dtos.ServiceInfo serviceInfo)
        {
            if (serviceInfoMap.TryGetValue(serviceInfo.GetKey(), out var oldService))
            {
            }

            if ((serviceInfo.hosts != null && serviceInfo.hosts.Any()) || !serviceInfo.Validate()) return oldService;

            serviceInfoMap[serviceInfo.GetKey()] = serviceInfo;

            bool changed = IsChangedServiceInfo(oldService, serviceInfo);

            if (string.IsNullOrWhiteSpace(serviceInfo.JsonFromServer))
            {
                serviceInfo.JsonFromServer = serviceInfo.ToJsonString();
            }

            if (changed)
            {
                _logger?.LogInformation("current ips:({0}) service: {1} -> {2}", serviceInfo.IpCount(), serviceInfo.GetKey(), serviceInfo.hosts.ToJsonString());

                DiskCache.WriteAsync(serviceInfo, cacheDir)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return serviceInfo;
        }

        private bool IsChangedServiceInfo(Dtos.ServiceInfo oldService, Dtos.ServiceInfo newService)
        {
            if (oldService == null)
            {
                _logger?.LogInformation("init new ips({0}) service: {1} -> {2}", newService.IpCount(), newService.GetKey(), newService.hosts.ToJsonString());
                return true;
            }

            if (oldService.lastRefTime > newService.lastRefTime)
            {
                _logger?.LogWarning("out of date data received, old-t: {0}, new-t: {1}", oldService.lastRefTime, newService.lastRefTime);
            }

            bool changed = false;

            var oldHostMap = oldService.hosts.ToDictionary(x => x.ToInetAddr());
            var newHostMap = newService.hosts.ToDictionary(x => x.ToInetAddr());

            var modHosts = newHostMap.Where(x => oldHostMap.ContainsKey(x.Key) && !x.Value.ToString().Equals(oldHostMap[x.Key].ToString()))
                   .Select(x => x.Value).ToList();
            var newHosts = newHostMap.Where(x => !oldHostMap.ContainsKey(x.Key))
                .Select(x => x.Value).ToList();
            var removeHosts = oldHostMap.Where(x => !newHostMap.ContainsKey(x.Key))
                .Select(x => x.Value).ToList();

            if (newHosts.Count > 0)
            {
                changed = true;
                _logger.LogInformation(
                    "new ips ({0}) service: {1} -> {2}",
                    newHosts.Count(),
                    newService.GetKey(),
                    newHosts.ToJsonString());
            }

            if (removeHosts.Count() > 0)
            {
                changed = true;
                _logger.LogInformation(
                  "removed ips ({0}) service: {1} -> {2}",
                  removeHosts.Count(),
                  newService.GetKey(),
                  removeHosts.ToJsonString());
            }

            if (modHosts.Count() > 0)
            {
                changed = true;
                _logger.LogInformation(
                 "modified ips ({0}) service: {1} -> {2}",
                 modHosts.Count(),
                 newService.GetKey(),
                 modHosts.ToJsonString());
            }

            return changed;
        }

        internal ConcurrentDictionary<string, Dtos.ServiceInfo> GetServiceInfoMap()
        {
            return serviceInfoMap;
        }

        private void InitCacheDir(string @namespace)
        {
            var jmSnapshotPath = System.Environment.GetEnvironmentVariable("JM.SNAPSHOT.PATH");
            if (string.IsNullOrWhiteSpace(jmSnapshotPath))
            {
                cacheDir = System.IO.Path.Combine(jmSnapshotPath, "nacos", "naming", @namespace);
            }
            else
            {
                cacheDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "nacos", "naming", @namespace);
            }
        }

        internal Dtos.ServiceInfo GetServiceInfo(string serviceName, string groupName, string clusters)
        {
            string groupedServiceName = NamingUtils.GetGroupedName(serviceName, groupName);
            string key = ServiceInfo.GetKey(groupedServiceName, clusters);

            /*if (failoverReactor.isFailoverSwitch())
            {
                return failoverReactor.getService(key);
            }*/
            return serviceInfoMap[key];
        }
    }
}
