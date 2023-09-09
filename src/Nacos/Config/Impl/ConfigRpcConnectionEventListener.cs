﻿namespace Nacos.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Nacos.Common;
    using Nacos.Logging;
    using Nacos.Remote;
    using System;
    using System.Collections.Concurrent;

    public class ConfigRpcConnectionEventListener : IConnectionEventListener
    {
        private readonly ILogger _logger = NacosLogManager.CreateLogger<ConfigRpcConnectionEventListener>();
        private readonly RpcClient _rpcClient;
        private readonly ConcurrentDictionary<string, CacheData> _cacheMap;
        private readonly BlockingCollection<object> _listenExecutebell;
        private object _bellItem = new object();

        public ConfigRpcConnectionEventListener(RpcClient rpcClientInner, ConcurrentDictionary<string, CacheData> cacheMap, BlockingCollection<object> listenExecutebell)
        {
            _rpcClient = rpcClientInner;
            _cacheMap = cacheMap;
            _listenExecutebell = listenExecutebell;
        }

        public void OnConnected()
        {
            _logger?.LogInformation("[{0}] Connected,notify listen context...", _rpcClient.GetName());
            NotifyListenConfig();
        }

        private void NotifyListenConfig()
        {
            _listenExecutebell.TryAdd(_bellItem);
        }

        public void OnDisConnected()
        {
            if (_rpcClient.GetLabels().TryGetValue("taskId", out var taskId))
            {
                _logger?.LogInformation("[{0}] DisConnected,clear listen context...", _rpcClient.GetName());

                var values = _cacheMap.Values;

                foreach (var cacheData in values)
                {
                    if (cacheData.TaskId.Equals(Convert.ToInt32(taskId)))
                    {
                        cacheData.IsListenSuccess = false;
                        continue;
                    }

                    cacheData.IsListenSuccess = false;
                }
            }
        }
    }
}
