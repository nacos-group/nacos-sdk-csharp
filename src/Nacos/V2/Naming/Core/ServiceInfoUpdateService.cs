namespace Nacos.V2.Naming.Core
{
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Event;
    using Nacos.V2.Naming.Remote;
    using Nacos.V2.Naming.Utils;
    using System.Collections.Generic;
    using System.Threading;

    public class ServiceInfoUpdateService
    {
        /*private static readonly long DEFAULT_DELAY = 1000L;*/

        private Dictionary<string, Timer> futureMap = new Dictionary<string, Timer>();

        private ServiceInfoHolder serviceInfoHolder;

        /*private Timer executor;*/

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

            if (futureMap.TryGetValue(serviceKey, out var timer)) return;

            lock (futureMap)
            {
                if (futureMap.TryGetValue(serviceKey, out timer)) return;

                // ToDo update futureMap
            }
        }

        public void StopUpdateIfContain(string serviceName, string groupName, string clusters)
        {
            string serviceKey = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);
            if (!futureMap.TryGetValue(serviceKey, out var timer)) return;

            lock (futureMap)
            {
                if (!futureMap.TryGetValue(serviceKey, out timer)) return;

                futureMap.Remove(serviceKey);
            }
        }
    }
}
