namespace Nacos.V2.Config.Impl
{
    using Nacos.V2.Remote;
    using System;
    using System.Collections.Concurrent;

    public class ConfigRpcConnectionEventListener : IConnectionEventListener
    {
        private readonly RpcClient _rpcClient;
        private readonly ConcurrentDictionary<string, CacheData> _cacheMap;

        public ConfigRpcConnectionEventListener(RpcClient rpcClientInner, ConcurrentDictionary<string, CacheData> cacheMap)
        {
            this._rpcClient = rpcClientInner;
            this._cacheMap = cacheMap;
        }

        public void OnConnected()
        {
        }

        public void OnDisConnected()
        {
            if (_rpcClient.GetLabels().TryGetValue("taskId", out var taskId))
            {
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
