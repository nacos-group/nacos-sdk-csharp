namespace Nacos.V2.Naming.Core
{
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Event;
    using Nacos.V2.Naming.Remote;
    using Nacos.V2.Naming.Utils;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;

    public class ServiceInfoUpdateService
    {
        private static readonly int DEFAULT_DELAY = 1000;

        private static readonly int DEFAULT_UPDATE_CACHE_TIME_MULTIPLE = 6;

        private ConcurrentDictionary<string, Task> _updatingMap = new ConcurrentDictionary<string, Task>();

        private ServiceInfoHolder serviceInfoHolder;

        private INamingClientProxy namingClientProxy;

        private InstancesChangeNotifier changeNotifier;

        public ServiceInfoUpdateService(NacosSdkOptions properties, ServiceInfoHolder serviceInfoHolder,
            INamingClientProxy namingClientProxy, InstancesChangeNotifier changeNotifier)
        {
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
        }

        private async Task RunUpdateTask(string serviceName, string groupName, string clusters)
        {
            int delayTime = -1;
            var serviceKey = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);

            try
            {
                if (!changeNotifier.IsSubscribed(groupName, serviceName, clusters) && !_updatingMap.ContainsKey(serviceKey))
                {
                    // TODO logger
                    return;
                }

                if (!serviceInfoHolder.GetServiceInfoMap().TryGetValue(serviceKey, out var serviceObj))
                {
                    serviceObj = await namingClientProxy.QueryInstancesOfService(serviceName, groupName, clusters, 0, false);
                    serviceInfoHolder.ProcessServiceInfo(serviceObj);
                    delayTime = DEFAULT_DELAY;

                    // TODO lastRefTime serviceObj.LastRefTime
                    return;
                }

                if (serviceObj.LastRefTime <= 0)
                {
                    serviceObj = await namingClientProxy.QueryInstancesOfService(serviceName, groupName, clusters, 0, false);
                    serviceInfoHolder.ProcessServiceInfo(serviceObj);
                }

                // TODO lastRefTime serviceObj.LastRefTime
                if (serviceObj.Hosts == null || serviceObj.Hosts.Any())
                {
                    // incFailCount
                    return;
                }

                delayTime = (int)serviceObj.CacheMillis * DEFAULT_UPDATE_CACHE_TIME_MULTIPLE;

                // resetFailCount
            }
            catch (System.Exception)
            {
                // logger
            }
            finally
            {
                // next
            }
        }

        public void StopUpdateIfContain(string serviceName, string groupName, string clusters)
        {
            string serviceKey = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);

            _updatingMap.TryRemove(serviceKey, out _);
        }
    }
}
