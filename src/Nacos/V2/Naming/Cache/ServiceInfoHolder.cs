namespace Nacos.V2.Naming.Cache
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Naming.Backups;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using Nacos.V2.Naming.Event;
    using Nacos.V2.Utils;
    using System.IO;
    using Nacos.V2.Naming.Dtos;

    public class ServiceInfoHolder : IDisposable
    {
        private static readonly string FILE_PATH_NACOS = "nacos";
        private static readonly string FILE_PATH_NAMING = "naming";

        private readonly ILogger _logger;
        private readonly FailoverReactor _failoverReactor;
        private readonly ConcurrentDictionary<string, Dtos.ServiceInfo> _serviceInfoMap;

        private InstancesChangeNotifier _notifier;
        private string cacheDir = string.Empty;
        private bool _pushEmptyProtection;

        public ServiceInfoHolder(ILogger logger, string @namespace, NacosSdkOptions nacosOptions, InstancesChangeNotifier notifier = null)
        {
            this._logger = logger;
            this._notifier = notifier;

            InitCacheDir(@namespace, nacosOptions);

            if (IsLoadCacheAtStart(nacosOptions))
            {
                var data = DiskCache.ReadAsync(this.cacheDir).ConfigureAwait(false).GetAwaiter().GetResult();
                this._serviceInfoMap = new ConcurrentDictionary<string, Dtos.ServiceInfo>(data);
            }
            else
            {
                this._serviceInfoMap = new ConcurrentDictionary<string, Dtos.ServiceInfo>();
            }

            this._failoverReactor = new FailoverReactor(_logger, this, cacheDir);
            this._pushEmptyProtection = nacosOptions.NamingPushEmptyProtection;
        }

        private bool IsLoadCacheAtStart(NacosSdkOptions nacosOptions)
        {
            bool loadCacheAtStart = false;
            if (nacosOptions != null && nacosOptions.NamingLoadCacheAtStart.IsNotNullOrWhiteSpace())
            {
                loadCacheAtStart = Convert.ToBoolean(nacosOptions.NamingLoadCacheAtStart);
            }

            return loadCacheAtStart;
        }

        internal Dtos.ServiceInfo ProcessServiceInfo(string json)
        {
            var serviceInfo = json.ToObj<Dtos.ServiceInfo>();
            serviceInfo.JsonFromServer = json;
            return ProcessServiceInfo(serviceInfo);
        }

        internal Dtos.ServiceInfo ProcessServiceInfo(Dtos.ServiceInfo serviceInfo)
        {
            if (serviceInfo.GetKey().IsNullOrWhiteSpace()) return null;

            _serviceInfoMap.TryGetValue(serviceInfo.GetKey(), out var oldService);

            if (IsEmptyOrErrorPush(serviceInfo)) return oldService;

            _serviceInfoMap.AddOrUpdate(serviceInfo.GetKey(), serviceInfo, (x, y) => serviceInfo);

            bool changed = IsChangedServiceInfo(oldService, serviceInfo);

            if (serviceInfo.JsonFromServer.IsNullOrWhiteSpace())
            {
                serviceInfo.JsonFromServer = serviceInfo.ToJsonString();
            }

            if (changed)
            {
                _logger?.LogInformation("current ips:({0}) service: {1} -> {2}", serviceInfo.IpCount(), serviceInfo.GetKey(), serviceInfo.Hosts.ToJsonString());

                if (_notifier != null)
                {
                    _notifier.OnEvent(new InstancesChangeEvent(serviceInfo.Name, serviceInfo.GroupName, serviceInfo.Clusters, serviceInfo.Hosts));
                }

                DiskCache.WriteAsync(serviceInfo, cacheDir)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return serviceInfo;
        }

        private bool IsEmptyOrErrorPush(Dtos.ServiceInfo serviceInfo)
            => serviceInfo.Hosts == null || !serviceInfo.Hosts.Any() || (_pushEmptyProtection && !serviceInfo.Validate());

        private bool IsChangedServiceInfo(Dtos.ServiceInfo oldService, Dtos.ServiceInfo newService)
        {
            if (oldService == null)
            {
                _logger?.LogInformation("init new ips({0}) service: {1} -> {2}", newService.IpCount(), newService.GetKey(), newService.Hosts.ToJsonString());
                return true;
            }

            if (oldService.LastRefTime > newService.LastRefTime)
            {
                _logger?.LogWarning("out of date data received, old-t: {0}, new-t: {1}", oldService.LastRefTime, newService.LastRefTime);
            }

            bool changed = false;

            var oldHostMap = oldService.Hosts.ToDictionary(x => x.ToInetAddr());
            var newHostMap = newService.Hosts.ToDictionary(x => x.ToInetAddr());

            var modHosts = newHostMap.Where(x => oldHostMap.ContainsKey(x.Key) && !x.Value.ToString().Equals(oldHostMap[x.Key].ToString()))
                   .Select(x => x.Value).ToList();
            var newHosts = newHostMap.Where(x => !oldHostMap.ContainsKey(x.Key))
                .Select(x => x.Value).ToList();
            var removeHosts = oldHostMap.Where(x => !newHostMap.ContainsKey(x.Key))
                .Select(x => x.Value).ToList();

            if (newHosts.Count > 0)
            {
                changed = true;
                _logger?.LogInformation(
                    "new ips ({0}) service: {1} -> {2}",
                    newHosts.Count(),
                    newService.GetKey(),
                    newHosts.ToJsonString());
            }

            if (removeHosts.Count() > 0)
            {
                changed = true;
                _logger?.LogInformation(
                  "removed ips ({0}) service: {1} -> {2}",
                  removeHosts.Count(),
                  newService.GetKey(),
                  removeHosts.ToJsonString());
            }

            if (modHosts.Count() > 0)
            {
                changed = true;
                _logger?.LogInformation(
                 "modified ips ({0}) service: {1} -> {2}",
                 modHosts.Count(),
                 newService.GetKey(),
                 modHosts.ToJsonString());
            }

            return changed;
        }

        internal ConcurrentDictionary<string, Dtos.ServiceInfo> GetServiceInfoMap() => _serviceInfoMap;

        private void InitCacheDir(string @namespace, NacosSdkOptions options)
        {
            var jmSnapshotPath = EnvUtil.GetEnvValue("JM.SNAPSHOT.PATH");

            string namingCacheRegistryDir = string.Empty;
            if (options.NamingCacheRegistryDir.IsNotNullOrWhiteSpace())
            {
                namingCacheRegistryDir = options.NamingCacheRegistryDir;
            }

            if (!string.IsNullOrWhiteSpace(jmSnapshotPath))
            {
                cacheDir = Path.Combine(jmSnapshotPath, FILE_PATH_NACOS, FILE_PATH_NAMING, @namespace);
            }
            else
            {
                cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), FILE_PATH_NACOS, FILE_PATH_NAMING, @namespace);
            }
        }

        internal Dtos.ServiceInfo GetServiceInfo(string serviceName, string groupName, string clusters)
        {
            _logger?.LogDebug("failover-mode:{0}", _failoverReactor.IsFailoverSwitch());
            string groupedServiceName = NamingUtils.GetGroupedName(serviceName, groupName);
            string key = ServiceInfo.GetKey(groupedServiceName, clusters);

            if (_failoverReactor.IsFailoverSwitch())
            {
                return _failoverReactor.GetService(key);
            }

            return _serviceInfoMap.TryGetValue(key, out var serviceInfo) ? serviceInfo : null;
        }

        public void Dispose()
        {
            _logger?.LogInformation("{0} do shutdown begin", nameof(ServiceInfoHolder));
            _failoverReactor?.Dispose();
            _logger?.LogInformation("{0} do shutdown stop", nameof(ServiceInfoHolder));
        }
    }
}
