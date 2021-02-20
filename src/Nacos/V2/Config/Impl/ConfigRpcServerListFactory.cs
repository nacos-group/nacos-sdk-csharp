namespace Nacos.V2.Config.Impl
{
    using Nacos.V2.Remote;
    using System.Collections.Generic;

    public class ConfigRpcServerListFactory : IServerListFactory
    {
        private readonly ServerListManager _serverListManager;

        public ConfigRpcServerListFactory(ServerListManager serverListManager)
        {
            this._serverListManager = serverListManager;
        }

        public string GenNextServer() => _serverListManager.GetNextServerAddr();

        public string GetCurrentServer() => _serverListManager.GetCurrentServerAddr();

        public List<string> GetServerList() => _serverListManager.GetServerUrls();
    }
}
