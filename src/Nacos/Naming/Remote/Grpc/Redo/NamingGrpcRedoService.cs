﻿namespace Nacos.Naming.Remote.Grpc.Redo
{
    using Microsoft.Extensions.Logging;
    using Nacos.Logging;
    using Nacos.Naming.Dtos;
    using Nacos.Naming.Remote.Grpc;
    using Nacos.Naming.Utils;
    using Nacos.Remote;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class NamingGrpcRedoService : IConnectionEventListener, IDisposable
    {
        private readonly ILogger _logger = NacosLogManager.CreateLogger<NamingGrpcRedoService>();
        private readonly ConcurrentDictionary<string, InstanceRedoData> _registeredInstances;
        private readonly ConcurrentDictionary<string, SubscriberRedoData> _subscribes;

        private Timer _timer;
        private long _connected = 0;
        private static readonly long DEFAULT_REDO_DELAY = 3000L;

        public NamingGrpcRedoService(NamingGrpcClientProxy clientProxy)
        {
            _registeredInstances = new ConcurrentDictionary<string, InstanceRedoData>();
            _subscribes = new ConcurrentDictionary<string, SubscriberRedoData>();
            _timer = new Timer(
                async x => await new RedoScheduledTask(clientProxy, this).Run().ConfigureAwait(false),
                null,
                TimeSpan.FromMilliseconds(DEFAULT_REDO_DELAY),
                TimeSpan.FromMilliseconds(DEFAULT_REDO_DELAY));
        }

        public ConcurrentDictionary<string, InstanceRedoData> GetRegisteredInstances()
        {
            return _registeredInstances;
        }

        public InstanceRedoData GetRegisteredInstancesByKey(string combinedServiceName)
        {
            return _registeredInstances.TryGetValue(combinedServiceName, out var data)
                ? data
                : null;
        }

        public void OnConnected()
        {
            Interlocked.Exchange(ref _connected, 1);
            _logger?.LogInformation("Grpc connection connect");
        }

        public void OnDisConnected()
        {
            Interlocked.Exchange(ref _connected, 0);
            _logger?.LogWarning("Grpc connection disconnect, mark to redo");

            _registeredInstances.Values.ToList().ForEach(d => d.Registered = false);
            _subscribes.Values.ToList().ForEach(d => d.Registered = false);

            _logger?.LogWarning("mark to redo completed");
        }

        public bool IsConnected() => Interlocked.Read(ref _connected) == 1;

        /// <summary>
        /// Cache registered instance for redo.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="instance">registered instance</param>
        public void CacheInstanceForRedo(string serviceName, string groupName, Instance instance)
        {
            string key = NamingUtils.GetGroupedName(serviceName, groupName);
            var redoData = InstanceRedoData.Build(serviceName, groupName, instance);

            _registeredInstances.AddOrUpdate(key, redoData, (x, y) => redoData);
        }

        public void CacheInstanceForRedo(string serviceName, string groupName, List<Instance> instances)
        {
            string key = NamingUtils.GetGroupedName(serviceName, groupName);
            var redoData = BatchInstanceRedoData.Build(serviceName, groupName, instances);

            _registeredInstances.AddOrUpdate(key, redoData, (x, y) => redoData);
        }

        /// <summary>
        /// Instance register successfully, mark registered status as true
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        public void InstanceRegistered(string serviceName, string groupName)
        {
            string key = NamingUtils.GetGroupedName(serviceName, groupName);

            if (_registeredInstances.TryGetValue(key, out var data))
            {
                data.Registered = true;
            }
        }

        /// <summary>
        /// Instance deregister, mark unregistering status as true.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        public void InstanceDeregister(string serviceName, string groupName)
        {
            string key = NamingUtils.GetGroupedName(serviceName, groupName);

            if (_registeredInstances.TryGetValue(key, out var data))
            {
                data.Unregistering = true;
            }
        }

        /// <summary>
        /// Judge subscriber has registered to server.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="cluster">cluster</param>
        /// <returns>true if subscribed, otherwise false</returns>
        public bool IsSubscriberRegistered(string serviceName, string groupName, string cluster)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), cluster);
            return _subscribes.TryGetValue(key, out var data) && data != null && data.Registered;
        }

        /// <summary>
        /// Remove registered instance for redo.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        public void RemoveInstanceForRedo(string serviceName, string groupName)
        {
            string key = NamingUtils.GetGroupedName(serviceName, groupName);

            if (_registeredInstances.TryGetValue(key, out var data) && data != null && !data.ExpectedRegistered)
            {
                _registeredInstances.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Find all instance redo data which need do redo.
        /// </summary>
        /// <returns>set of InstanceRedoData need to do redo.</returns>
        public HashSet<InstanceRedoData> FindInstanceRedoData()
        {
            var result = new HashSet<InstanceRedoData>();

            foreach (var item in _registeredInstances.Values)
            {
                if (item.IsNeedRedo())
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Cache subscriber for redo.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="cluster">cluster</param>
        public void CacheSubscriberForRedo(string serviceName, string groupName, string cluster)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), cluster);
            var redoData = SubscriberRedoData.Build(serviceName, groupName, cluster);

            _subscribes.AddOrUpdate(key, redoData, (x, y) => redoData);
        }

        /// <summary>
        /// Subscriber register successfully, mark registered status as true.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="cluster">cluster</param>
        public void SubscriberRegistered(string serviceName, string groupName, string cluster)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), cluster);

            if (_subscribes.TryGetValue(key, out var data))
            {
                data.Registered = true;
            }
        }

        /// <summary>
        /// Subscriber deregister, mark unregistering status as true.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="cluster">cluster</param>
        public void SubscriberDeregister(string serviceName, string groupName, string cluster)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), cluster);

            if (_subscribes.TryGetValue(key, out var data))
            {
                data.Unregistering = true;
                data.ExpectedRegistered = false;
            }
        }

        /// <summary>
        /// Remove subscriber for redo.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="cluster">cluster</param>
        public void RemoveSubscriberForRedo(string serviceName, string groupName, string cluster)
        {
            string key = ServiceInfo.GetKey(NamingUtils.GetGroupedName(serviceName, groupName), cluster);

            if (_subscribes.TryGetValue(key, out var data) && data != null && !data.ExpectedRegistered)
            {
                _subscribes.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Find all subscriber redo data which need do redo.
        /// </summary>
        /// <returns>set of SubscriberRedoData need to do redo.</returns>
        public HashSet<SubscriberRedoData> FindSubscriberRedoData()
        {
            var result = new HashSet<SubscriberRedoData>();

            foreach (var item in _subscribes.Values)
            {
                if (item.IsNeedRedo())
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public void Dispose()
        {
            _logger?.LogInformation("Shutdown grpc redo service executor ");

            _registeredInstances.Clear();
            _subscribes.Clear();
            _timer?.Dispose();
        }
    }
}
