namespace Nacos.V2.Naming.Remote.Grpc
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Remote;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class NamingGrpcRedoService : IConnectionEventListener, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, InstanceRedoData> _registeredInstances;
        private readonly ConcurrentDictionary<string, SubscriberRedoData> _subscribes;

        private Timer _timer;
        private long _connected = 0;
        private static readonly long DEFAULT_REDO_DELAY = 3000L;

        public NamingGrpcRedoService(ILogger logger, NamingGrpcClientProxy clientProxy)
        {
            this._logger = logger;
            this._registeredInstances = new ConcurrentDictionary<string, InstanceRedoData>();
            this._subscribes = new ConcurrentDictionary<string, SubscriberRedoData>();
            this._timer = new Timer(
                async x => await new RedoScheduledTask(_logger, clientProxy, this).Run().ConfigureAwait(false),
                null,
                TimeSpan.FromMilliseconds(DEFAULT_REDO_DELAY),
                TimeSpan.FromMilliseconds(DEFAULT_REDO_DELAY));
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

            _registeredInstances.TryRemove(key, out _);
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

            _subscribes.TryRemove(key, out _);
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
