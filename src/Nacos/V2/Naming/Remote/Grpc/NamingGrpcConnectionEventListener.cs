namespace Nacos.V2.Naming.Remote.Grpc
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Remote;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class NamingGrpcConnectionEventListener : IConnectionEventListener
    {
        private ILogger _logger;
        private NamingGrpcClientProxy _clientProxy;

        private ConcurrentDictionary<string, Instance> _registeredInstanceCached = new ConcurrentDictionary<string, Instance>();

        private HashSet<string> _subscribes = new HashSet<string>();

        public NamingGrpcConnectionEventListener(ILogger logger, NamingGrpcClientProxy clientProxy)
        {
            this._logger = logger;
            this._clientProxy = clientProxy;
        }

        public void OnConnected()
        {
            RedoSubscribe();
            RedoRegisterEachService();
        }

        private void RedoRegisterEachService()
        {
            _logger?.LogInformation("Grpc re-connect, redo register services");

            foreach (var item in _registeredInstanceCached)
            {
                var serviceName = NamingUtils.GetServiceName(item.Key);
                var groupName = NamingUtils.GetGroupName(item.Key);
                RedoRegisterEachInstance(serviceName, groupName, item.Value);
            }
        }

        private void RedoRegisterEachInstance(string serviceName, string groupName, Instance instance)
        {
            try
            {
                _clientProxy.RegisterServiceAsync(serviceName, groupName, instance)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger?.LogWarning(e, "redo register for service {0}@@{1} failed", groupName, serviceName);
            }
        }

        private void RedoSubscribe()
        {
            _logger?.LogInformation("Grpc re-connect, redo subscribe services");

            foreach (var item in _subscribes)
            {
                var serviceInfo = ServiceInfo.FromKey(item);
                try
                {
                    _clientProxy.Subscribe(serviceInfo.Name, serviceInfo.GroupName, serviceInfo.Clusters)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "re subscribe service {0} failed", serviceInfo.Name);
                }
            }
        }

        public void OnDisConnected()
        {
            _logger?.LogWarning("Grpc connection disconnect");
        }

        internal void RemoveSubscriberForRedo(string fullServiceName, string clusters)
        {
            _subscribes.Remove(ServiceInfo.GetKey(fullServiceName, clusters));
        }

        internal void CacheSubscriberForRedo(string fullServiceName, string clusters)
        {
            _subscribes.Add(ServiceInfo.GetKey(fullServiceName, clusters));
        }

        internal void CacheInstanceForRedo(string serviceName, string groupName, Instance instance)
        {
            string key = NamingUtils.GetGroupedName(serviceName, groupName);

            _registeredInstanceCached[key] = instance;
        }

        internal void RemoveInstanceForRedo(string serviceName, string groupName, Instance instance)
        {
            string key = NamingUtils.GetGroupedName(serviceName, groupName);

            _registeredInstanceCached.TryRemove(key, out _);
        }
    }
}
