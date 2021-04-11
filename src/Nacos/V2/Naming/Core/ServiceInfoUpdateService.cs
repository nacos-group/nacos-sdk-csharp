namespace Nacos.V2.Naming.Core
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Event;
    using Nacos.V2.Naming.Remote;
    using Nacos.V2.Naming.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ServiceInfoUpdateService
    {
        private static readonly int DEFAULT_DELAY = 1000;

        private static readonly int DEFAULT_UPDATE_CACHE_TIME_MULTIPLE = 6;

        private ConcurrentDictionary<string, Timer> _timerMap = new ConcurrentDictionary<string, Timer>();
        private ConcurrentDictionary<string, Task> _updatingMap = new ConcurrentDictionary<string, Task>();

        private readonly ILogger _logger;

        private ServiceInfoHolder serviceInfoHolder;

        private INamingClientProxy namingClientProxy;

        private InstancesChangeNotifier changeNotifier;

        public ServiceInfoUpdateService(ILogger logger, NacosSdkOptions properties, ServiceInfoHolder serviceInfoHolder,
            INamingClientProxy namingClientProxy, InstancesChangeNotifier changeNotifier)
        {
            this._logger = logger;
            this.serviceInfoHolder = serviceInfoHolder;
            this.namingClientProxy = namingClientProxy;
            this.changeNotifier = changeNotifier;
        }

        public void ScheduleUpdateIfAbsent(string serviceName, string groupName, string clusters)
        {
            string serviceKey = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);

            var task = new TaskCompletionSource<bool>();
            if (_updatingMap.TryAdd(serviceKey, task.Task))
            {
                _ = RunUpdateTask(serviceName, groupName, clusters);
                task.SetResult(true);
            }
            else
            {
                // hold a moment waiting for update finish
                if (_updatingMap.TryGetValue(serviceKey, out var waitTask)) waitTask.Wait(DEFAULT_DELAY);
            }

            if (_timerMap.ContainsKey(serviceKey)) return;

            var t = UpdateTask(serviceName, groupName, clusters);
            _timerMap.TryAdd(serviceKey, t);
        }

        private async Task RunUpdateTask(string serviceName, string groupName, string clusters)
        {
            try
            {
                var serviceObj = await namingClientProxy.QueryInstancesOfService(serviceName, groupName, clusters, 0, false);

                if (serviceObj != null)
                {
                    serviceInfoHolder.ProcessServiceInfo(serviceObj);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "[NA] failed to update serviceName: {0}", NamingUtils.GetGroupedName(serviceName, groupName));
            }
        }

        private Timer UpdateTask(string serviceName, string groupName, string clusters)
        {
            return new Timer(
                async x =>
            {
                var state = x as UpdateState;

                int delayTime = -1;
                long lastRefTime = long.MaxValue;
                int failCount = 0;
                var serviceKey = ServiceInfo.GetKey(NamingUtils.GetGroupedName(state.ServiceName, state.GroupName), state.Clusters);
                _timerMap.TryGetValue(serviceKey, out var self);
                try
                {
                    if (!changeNotifier.IsSubscribed(state.GroupName, state.ServiceName, state.Clusters) && !_updatingMap.ContainsKey(serviceKey))
                    {
                        _logger?.LogInformation("update task is stopped, service:{0}, clusters:{1}", NamingUtils.GetGroupedName(state.ServiceName, state.GroupName), state.Clusters);
                        return;
                    }

                    if (!serviceInfoHolder.GetServiceInfoMap().TryGetValue(serviceKey, out var serviceObj))
                    {
                        serviceObj = await namingClientProxy.QueryInstancesOfService(state.ServiceName, state.GroupName, state.Clusters, 0, false);
                        serviceInfoHolder.ProcessServiceInfo(serviceObj);
                        delayTime = DEFAULT_DELAY;
                        lastRefTime = serviceObj.LastRefTime;
                        return;
                    }

                    if (serviceObj.LastRefTime <= lastRefTime)
                    {
                        serviceObj = await namingClientProxy.QueryInstancesOfService(serviceName, groupName, clusters, 0, false);
                        serviceInfoHolder.ProcessServiceInfo(serviceObj);
                    }

                    lastRefTime = serviceObj.LastRefTime;
                    if (serviceObj.Hosts == null || serviceObj.Hosts.Any())
                    {
                        IncFailCount(ref failCount);
                        return;
                    }

                    delayTime = (int)serviceObj.CacheMillis * DEFAULT_UPDATE_CACHE_TIME_MULTIPLE;
                    ResetFailCount(ref failCount);
                }
                catch (Exception ex)
                {
                    IncFailCount(ref failCount);
                    _logger?.LogWarning(ex, "[NA] failed to update serviceName: {0}", NamingUtils.GetGroupedName(state.ServiceName, state.GroupName));
                }
                finally
                {
                    self?.Change(Math.Min(delayTime << failCount, DEFAULT_DELAY * 60), Timeout.Infinite);
                }
            }, new UpdateState(serviceName, groupName, clusters), DEFAULT_DELAY * 60, Timeout.Infinite);
        }

        private void IncFailCount(ref int failCount)
        {
            int limit = 6;
            if (failCount == limit) return;

            failCount++;
        }

        private void ResetFailCount(ref int failCount) => failCount = 0;

        public void StopUpdateIfContain(string serviceName, string groupName, string clusters)
        {
            string serviceKey = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);

            _updatingMap.TryRemove(serviceKey, out _);
        }

        public class UpdateState
        {
            public string ServiceName { get; set; }

            public string Clusters { get; set; }

            public string GroupName { get; set; }

            public long LastRefTime { get; set; } = long.MaxValue;

            public UpdateState(string serviceName, string groupName, string clusters)
            {
                this.ServiceName = serviceName;
                this.GroupName = groupName;
                this.Clusters = clusters;
            }
        }
    }
}
