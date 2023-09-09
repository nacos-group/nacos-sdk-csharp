﻿namespace Nacos.Naming.Cache
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.IO;
    using Nacos.Naming.Backups;
    using Nacos.Naming.Dtos;
    using Nacos.Naming.Event;
    using Nacos.Naming.Utils;
    using Nacos.Utils;
    using Nacos;
    using Nacos.Logging;

    public class ServiceInfoHolder : IDisposable
    {
        private static readonly string FILE_PATH_NACOS = "nacos";
        private static readonly string FILE_PATH_NAMING = "naming";

        private readonly ILogger _logger = NacosLogManager.CreateLogger<ServiceInfoHolder>();
        private readonly FailoverReactor _failoverReactor;
        private readonly ConcurrentDictionary<string, Dtos.ServiceInfo> _serviceInfoMap;
        private readonly NacosSdkOptions _options;

        private InstancesChangeNotifier _notifier;
        private string cacheDir = string.Empty;
        private bool _pushEmptyProtection;

        public ServiceInfoHolder(string @namespace, NacosSdkOptions nacosOptions, InstancesChangeNotifier notifier = null)
        {
            _notifier = notifier;
            _options = nacosOptions;

            InitCacheDir(@namespace, nacosOptions);

            if (IsLoadCacheAtStart(nacosOptions))
            {
                var data = DiskCache.ReadAsync(cacheDir).ConfigureAwait(false).GetAwaiter().GetResult();
                _serviceInfoMap = new ConcurrentDictionary<string, Dtos.ServiceInfo>(data);
            }
            else
            {
                _serviceInfoMap = new ConcurrentDictionary<string, Dtos.ServiceInfo>();
            }

            _failoverReactor = new FailoverReactor(this, cacheDir);
            _pushEmptyProtection = nacosOptions.NamingPushEmptyProtection;
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
                    var @event = new InstancesChangeEvent(serviceInfo.Name, serviceInfo.GroupName, serviceInfo.Clusters, serviceInfo.Hosts);

                    // grpc 和 udp 返回的数据格式不一样，需要对 udp 的方式进行兼容
                    // {"name":"DEFAULT_GROUP@@mysvc2","clusters":"","cacheMillis":10000,"hosts":[{"serviceName":"DEFAULT_GROUP@@mysvc2"}],.....}
                    if (!_options.NamingUseRpc)
                    {
                        @event.ServiceName = NamingUtils.GetServiceName(serviceInfo.Name);
                        @event.GroupName = NamingUtils.GetGroupName(serviceInfo.Name);
                    }

                    _notifier.OnEvent(@event);
                }

                DiskCache.WriteAsync(serviceInfo, cacheDir)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return serviceInfo;
        }

        private bool IsEmptyOrErrorPush(Dtos.ServiceInfo serviceInfo)
            => serviceInfo.Hosts == null || (_pushEmptyProtection && !serviceInfo.Validate());

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
                return false;
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
