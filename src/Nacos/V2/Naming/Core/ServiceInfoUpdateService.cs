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

    public class ServiceInfoUpdateService
    {
        private static readonly int DEFAULT_DELAY = 1000;
        private static readonly int DEFAULT_UPDATE_CACHE_TIME_MULTIPLE = 6;

        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, Timer> _timerMap;
        private readonly ServiceInfoHolder _serviceInfoHolder;
        private readonly INamingClientProxy _namingClientProxy;
        private readonly InstancesChangeNotifier _changeNotifier;

        public ServiceInfoUpdateService(ILogger logger, NacosSdkOptions properties, ServiceInfoHolder serviceInfoHolder,
            INamingClientProxy namingClientProxy, InstancesChangeNotifier changeNotifier)
        {
            this._logger = logger;
            this._timerMap = new ConcurrentDictionary<string, Timer>();
            this._serviceInfoHolder = serviceInfoHolder;
            this._namingClientProxy = namingClientProxy;
            this._changeNotifier = changeNotifier;
        }

        public void ScheduleUpdateIfAbsent(string serviceName, string groupName, string clusters)
        {
            string serviceKey = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);

            if (_timerMap.TryGetValue(serviceKey, out _)) return;

            var task = UpdateTask(serviceName, groupName, clusters);

            _timerMap.TryAdd(serviceKey, task);
        }

        private Timer UpdateTask(string serviceName, string groupName, string clusters)
        {
            return new Timer(
                async x =>
            {
                var state = x as UpdateModel;

                int delayTime = DEFAULT_DELAY;

                try
                {
                    if (!_changeNotifier.IsSubscribed(state.GroupName, state.ServiceName, state.Clusters) && !_timerMap.ContainsKey(state.ServiceKey))
                    {
                        _logger?.LogInformation("update task is stopped, service:{0}, clusters:{1}", state.GroupedServiceName, state.Clusters);
                        return;
                    }

                    if (!_serviceInfoHolder.GetServiceInfoMap().TryGetValue(state.ServiceKey, out var serviceObj))
                    {
                        serviceObj = await _namingClientProxy.QueryInstancesOfService(state.ServiceName, state.GroupName, state.Clusters, 0, false).ConfigureAwait(false);

                        _serviceInfoHolder.ProcessServiceInfo(serviceObj);
                        delayTime = DEFAULT_DELAY;
                        state.LastRefTime = serviceObj.LastRefTime;
                        return;
                    }

                    if (serviceObj.LastRefTime <= state.LastRefTime)
                    {
                        serviceObj = await _namingClientProxy.QueryInstancesOfService(serviceName, groupName, clusters, 0, false).ConfigureAwait(false);
                        _serviceInfoHolder.ProcessServiceInfo(serviceObj);
                    }

                    state.LastRefTime = serviceObj.LastRefTime;
                    if (serviceObj.Hosts == null || !serviceObj.Hosts.Any())
                    {
                        state.IncFailCount();
                        return;
                    }

                    delayTime = (int)serviceObj.CacheMillis * DEFAULT_UPDATE_CACHE_TIME_MULTIPLE;
                    state.ResetFailCount();
                }
                catch (Exception ex)
                {
                    state.IncFailCount();
                    _logger?.LogWarning(ex, "[NA] failed to update serviceName: {0}", NamingUtils.GetGroupedName(state.ServiceName, state.GroupName));
                }
                finally
                {
                    _timerMap.TryGetValue(state.ServiceKey, out var self);
                    var due = Math.Min(delayTime << state.FailCount, DEFAULT_DELAY * 60);

                    self?.Change(due, Timeout.Infinite);
                }
            }, new UpdateModel(serviceName, groupName, clusters), DEFAULT_DELAY, Timeout.Infinite);
        }

        public void StopUpdateIfContain(string serviceName, string groupName, string clusters)
        {
            string serviceKey = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);

            if (_timerMap.TryRemove(serviceKey, out var t))
            {
                t?.Change(Timeout.Infinite, Timeout.Infinite);
                t?.Dispose();

                _logger?.LogInformation("stop update task, servicekey:{0}", serviceKey);
            }
        }

        public class UpdateModel
        {
            public string ServiceName { get; set; }

            public string Clusters { get; set; }

            public string GroupName { get; set; }

            public long LastRefTime { get; set; } = long.MaxValue;

            public int FailCount { get; set; }

            public string ServiceKey { get; set; }

            public string GroupedServiceName { get; set; }

            public UpdateModel(string serviceName, string groupName, string clusters)
            {
                this.ServiceName = serviceName;
                this.GroupName = groupName;
                this.Clusters = clusters;
                this.GroupedServiceName = NamingUtils.GetGroupedName(serviceName, groupName);
                this.ServiceKey = ServiceInfo.GetKey(GroupedServiceName, clusters);
            }

            internal void IncFailCount()
            {
                int limit = 6;
                if (FailCount == limit) return;

                FailCount++;
            }

            internal void ResetFailCount() => FailCount = 0;
        }
    }
}
