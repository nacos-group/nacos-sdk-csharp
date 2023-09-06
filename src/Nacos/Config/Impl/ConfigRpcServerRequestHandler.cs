namespace Nacos.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Nacos.Config.Common;
    using Nacos.Remote;
    using Nacos.Remote.Requests;
    using Nacos.Utils;
    using Nacos.Remote.Responses;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Nacos.Common;

    public class ConfigRpcServerRequestHandler : IServerRequestHandler
    {
        private readonly ILogger _logger = NacosLogManager.CreateLogger<ConfigRpcServerRequestHandler>();

        private ConcurrentDictionary<string, CacheData> _cacheMap;
        private Func<Task> _func;

        public ConfigRpcServerRequestHandler(ConcurrentDictionary<string, CacheData> map, Func<Task> func)
        {
            _cacheMap = map;
            _func = func;
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
