namespace Nacos.V2.Config.Impl
{
    using Nacos.Utilities;
    using Nacos.V2.Remote;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using System;
    using System.Collections.Generic;

    public class ConfigRpcServerRequestHandler : IServerRequestHandler
    {
        private Dictionary<string, CacheData> _cacheMap;

        public ConfigRpcServerRequestHandler(Dictionary<string, CacheData> map)
        {
            this._cacheMap = map;
        }

        public CommonResponse RequestReply(CommonRequest request, CommonRequestMeta meta)
        {
            if (request is ConfigChangeNotifyRequest configChangeNotifyRequest)
            {
                string groupKey = GroupKey.GetKeyTenant(configChangeNotifyRequest.DataId, configChangeNotifyRequest.Group, configChangeNotifyRequest.Tenant);

                if (_cacheMap.TryGetValue(groupKey, out var cacheData))
                {
                    if (configChangeNotifyRequest.ContentPush
                        && cacheData.LastModifiedTs < configChangeNotifyRequest.LastModifiedTs)
                    {
                    }

                    cacheData.IsListenSuccess = false;
                }

                Console.WriteLine("Config RequestReply => {0}", request.ToJsonString());

                return new ConfigChangeNotifyResponse();
            }

            return null;
        }
    }
}
