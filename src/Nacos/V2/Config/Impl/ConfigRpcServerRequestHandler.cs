namespace Nacos.V2.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Remote;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    public class ConfigRpcServerRequestHandler : IServerRequestHandler
    {
        private readonly ILogger _logger;
        private ConcurrentDictionary<string, CacheData> _cacheMap;
        private Func<Task> _func;

        public ConfigRpcServerRequestHandler(ILogger logger, ConcurrentDictionary<string, CacheData> map, Func<Task> func)
        {
            this._logger = logger;
            this._cacheMap = map;
            this._func = func;
        }

        public CommonResponse RequestReply(CommonRequest request)
        {
            if (request is ConfigChangeNotifyRequest configChangeNotifyRequest)
            {
                string groupKey = GroupKey.GetKeyTenant(configChangeNotifyRequest.DataId, configChangeNotifyRequest.Group, configChangeNotifyRequest.Tenant);

                if (_cacheMap.TryGetValue(groupKey, out var cacheData))
                {
                    if (configChangeNotifyRequest.ContentPush
                        && cacheData.LastModifiedTs < configChangeNotifyRequest.LastModifiedTs)
                    {
                        cacheData.SetContent(configChangeNotifyRequest.Content);
                        cacheData.Type = configChangeNotifyRequest.Type;
                        cacheData.CheckListenerMd5();
                    }

                    cacheData.IsListenSuccess = false;

                    // notifyListenConfig
                    _func.Invoke().Wait();
                }

                _logger?.LogDebug("Config RequestReply => {0}", request.ToJsonString());

                return new ConfigChangeNotifyResponse();
            }

            return null;
        }
    }
}
