namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Nacos.Naming.Http;
    using Nacos.Utilities;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class HostReactor
    {
        private readonly ILogger _logger;

        private ConcurrentDictionary<string, ServiceInfo> _serviceInfoMap = new ConcurrentDictionary<string, ServiceInfo>();
        private ConcurrentDictionary<string, Task> _updatingMap = new ConcurrentDictionary<string, Task>();

        private readonly EventDispatcher _eventDispatcher;
        private readonly NamingProxy _proxy;
        private readonly NacosOptions _options;
        /*private readonly PushReceiver _pushReceiver;*/

        public HostReactor(
            ILoggerFactory loggerFactory,
            EventDispatcher eventDispatcher,
            NamingProxy proxy,
            NacosOptions options)
        {
            _logger = loggerFactory.CreateLogger<HostReactor>();
            _eventDispatcher = eventDispatcher;
            _proxy = proxy;
            _options = options;

            // At this time, push receiver using udp way, it's not stable and nacos server
            // has limititation on different clients, c# was not support, so disable here.
            // _pushReceiver = new PushReceiver(loggerFactory, this);
            Task.Factory.StartNew(
                async () => await UpdateTask().ConfigureAwait(false));
        }

        private ServiceInfo GetServiceInfo0(string serviceName, string clusters)
        {
            string key = ServiceInfo.GetKey(serviceName, clusters);

            return _serviceInfoMap.TryGetValue(key, out var serviceObj)
                ? serviceObj
                : null;
        }

        public ConcurrentDictionary<string, ServiceInfo> GetServiceInfoMap() => _serviceInfoMap;

        public async Task<ServiceInfo> GetServiceInfo(string serviceName, string clusters)
        {
            string key = ServiceInfo.GetKey(serviceName, clusters);

            ServiceInfo serviceObj = GetServiceInfo0(serviceName, clusters);

            if (serviceObj == null)
            {
                serviceObj = new ServiceInfo(serviceName, clusters);
                var task = new TaskCompletionSource<bool>();
                if (_updatingMap.TryAdd(key, task.Task))
                {
                    await UpdateServiceNowAsync(serviceName, clusters).ConfigureAwait(false);
                    task.SetResult(true);
                }
                else
                {
                    // hold a moment waiting for update finish
                    if (_updatingMap.TryGetValue(key, out var waitTask))
                    {
                        waitTask.Wait(1000);
                    }
                }
            }

            return serviceObj;
        }

        private async Task UpdateTask()
        {
            foreach (var kv in _serviceInfoMap)
            {
                var service = kv.Value;

                string key = ServiceInfo.GetKey(service.name, service.clusters);

                if (_updatingMap.ContainsKey(key))
                {
                    await UpdateServiceNowAsync(service.name, service.clusters).ConfigureAwait(false);
                }
            }

            await Task.Delay(5000).ConfigureAwait(false);
        }

        public async Task<ServiceInfo> GetServiceInfoDirectlyFromServerAsync(string serviceName, string clusters)
        {
            var result = await QueryListAsync(serviceName, clusters, 0, false).ConfigureAwait(false);

            return !string.IsNullOrWhiteSpace(result)
                ? result.ToObj<ServiceInfo>()
                : null;
        }

        public void ProcessServiceJson(string result)
        {
            var newService = result.ToObj<ServiceInfo>();
            _serviceInfoMap.TryGetValue(newService.GetKey(), out var oldService);
            if (newService.Hosts == null || !newService.Validate())
            {
                return;
            }

            var changed = false;

            if (oldService != null)
            {
                if (oldService.lastRefTime > newService.lastRefTime)
                {
                    _logger?.LogWarning("out of date data received, old-t: {0}, new-t: {1}", oldService.lastRefTime, newService.lastRefTime);
                }

                _serviceInfoMap.AddOrUpdate(newService.GetKey(), newService, (k, v) => newService);

                var oldMap = oldService.Hosts.ToDictionary(x => x.ToInetAddr());
                var newMap = newService.Hosts.ToDictionary(x => x.ToInetAddr());

                var modHosts = newMap.Where(x => oldMap.ContainsKey(x.Key) && !x.Value.ToString().Equals(oldMap[x.Key].ToString()))
                    .Select(x => x.Value).ToList();
                var newHosts = newMap.Where(x => !oldMap.ContainsKey(x.Key))
                    .Select(x => x.Value).ToList();
                var removeHosts = oldMap.Where(x => !newMap.ContainsKey(x.Key))
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

                if (newHosts.Count() > 0 || removeHosts.Count() > 0 || modHosts.Count() > 0)
                {
                    _eventDispatcher.ServiceChanged(newService);
                }
            }
            else
            {
                changed = true;
                _logger?.LogInformation(
                    "init new ips({0}) service: {1} -> {2}",
                    newService.IpCount(),
                    newService.GetKey(),
                    newService.Hosts.ToJsonString());
                _serviceInfoMap.TryAdd(newService.GetKey(), newService);
                _eventDispatcher.ServiceChanged(newService);
            }

            if (changed)
            {
                _logger?.LogInformation(
                    "current ips({0}) service: {1} -> {2}",
                    newService.IpCount(),
                    newService.GetKey(),
                    newService.Hosts.ToJsonString());
            }
        }

        private async Task UpdateServiceNowAsync(string serviceName, string clusters)
        {
            try
            {
                // disable upd
                // var port = _pushReveiver.GetUdpPort();
                var port = 0;
                var result = await QueryListAsync(serviceName, clusters, port, false).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    ProcessServiceJson(result);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[NA] failed to update serviceName: {0}", serviceName);
            }
        }

        private async Task<string> QueryListAsync(string serviceName, string clusters, int udpPort, bool healthyOnly)
        {
            var request = new ListInstancesRequest
            {
                ServiceName = serviceName,
                Clusters = clusters,
                NamespaceId = _options.Namespace,
                UdpPort = udpPort,
                HealthyOnly = healthyOnly,
                ClientIp = GetCurrentIp()
            };

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.INSTANCE_LIST, null, request.ToDict(), _options.DefaultTimeOut).ConfigureAwait(false);

            responseMessage.EnsureSuccessStatusCode();

            return await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private string GetCurrentIp()
        {
            var instanceIp = "127.0.0.1";

            try
            {
                foreach (var ipAddr in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (ipAddr.AddressFamily.ToString() != "InterNetwork") continue;

                    instanceIp = ipAddr.ToString();
                    break;
                }
            }
            catch
            {
                // ignored
            }

            return instanceIp;
        }
    }
}