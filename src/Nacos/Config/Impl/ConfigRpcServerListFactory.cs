namespace Nacos.Config.Impl
{
    using Nacos.Config.Abst;
    using Nacos.Remote;
    using System.Collections.Generic;

    public class ConfigRpcServerListFactory : IServerListFactory
    {
        private readonly IServerListManager _serverListManager;

        public ConfigRpcServerListFactory(IServerListManager serverListManager)
        {
            _serverListManager = serverListManager;
        }

        public string GenNextServer() => _serverListManager.GetNextServerAddr();

        public string GetCurrentServer() => _serverListManager.GetCurrentServerAddr();

        public List<string> GetServerList() => _serverListManager.GetServerUrls();
    }
}
