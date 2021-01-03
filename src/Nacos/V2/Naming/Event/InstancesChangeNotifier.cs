namespace Nacos.V2.Naming.Event
{
    using Nacos.V2.Naming.Utils;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class InstancesChangeNotifier
    {
        private ConcurrentDictionary<string, HashSet<IEventListener>> listenerMap = new ConcurrentDictionary<string, HashSet<IEventListener>>();

        private object obj = new object();


        public void RegisterListener(string groupName, string serviceName, string clusters, IEventListener listener)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);
            if (!listenerMap.TryGetValue(key, out var eventListeners))
            {
                lock (obj)
                {
                    eventListeners = listenerMap[key];
                    if (eventListeners == null)
                    {
                        eventListeners = new HashSet<IEventListener>();
                        listenerMap[key] = eventListeners;
                    }
                }
            }

            eventListeners.Add(listener);
        }

        public void DeregisterListener(string groupName, string serviceName, string clusters, IEventListener listener)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);

            if (!listenerMap.TryGetValue(key, out var eventListeners)) return;

            eventListeners.Remove(listener);

            if (eventListeners == null || !eventListeners.Any())
            {
                listenerMap.TryRemove(key, out _);
            }
        }

        public bool IsSubscribed(string groupName, string serviceName, string clusters)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), clusters);

            return listenerMap.TryGetValue(key, out var eventListeners)
                && eventListeners != null && eventListeners.Any();
        }

        public List<Nacos.V2.Naming.Dtos.ServiceInfo> GetSubscribeServices()
        {
            List<Nacos.V2.Naming.Dtos.ServiceInfo> serviceInfos = new List<Nacos.V2.Naming.Dtos.ServiceInfo>();
            foreach (var key in listenerMap.Keys)
            {
                serviceInfos.Add(Nacos.V2.Naming.Dtos.ServiceInfo.FromKey(key));
            }

            return serviceInfos;
        }

        public void OnEvent(InstancesChangeEvent @event)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(@event.ServiceName, @event.GroupName), @event.Clusters);

            if (!listenerMap.TryGetValue(key, out var eventListeners)) return;


            foreach (var listener in eventListeners) listener.OnEvent(@event);
        }
    }
}
