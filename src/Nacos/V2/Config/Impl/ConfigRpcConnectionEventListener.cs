namespace Nacos.V2.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Remote;
    using System;
    using System.Collections.Concurrent;

    public class ConfigRpcConnectionEventListener : IConnectionEventListener
    {
        private readonly ILogger _logger;
        private readonly RpcClient _rpcClient;
        private readonly ConcurrentDictionary<string, CacheData> _cacheMap;
        private readonly BlockingCollection<object> _listenExecutebell;
        private object _bellItem = new object();

        public ConfigRpcConnectionEventListener(ILogger logger, RpcClient rpcClientInner, ConcurrentDictionary<string, CacheData> cacheMap, BlockingCollection<object> listenExecutebell)
        {
            this._logger = logger;
            this._rpcClient = rpcClientInner;
            this._cacheMap = cacheMap;
            this._listenExecutebell = listenExecutebell;
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
