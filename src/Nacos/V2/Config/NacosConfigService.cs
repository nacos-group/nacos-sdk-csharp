namespace Nacos.V2.Config
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class NacosConfigService : INacosConfigService
    {
        public Task AddListener(string dataId, string group, IListener listener)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetConfig(string dataId, string group, long timeoutMs)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetConfigAndSignListener(string dataId, string group, long timeoutMs, IListener listener)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetServerStatus()
        {
            throw new NotImplementedException();
        }

        public Task<bool> PublishConfig(string dataId, string group, string content)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PublishConfig(string dataId, string group, string content, string type)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveConfig(string dataId, string group)
        {
            throw new NotImplementedException();
        }

        public Task RemoveListener(string dataId, string group, IListener listener)
        {
            throw new NotImplementedException();
        }

        public Task ShutDown()
        {
            throw new NotImplementedException();
        }
    }
}
