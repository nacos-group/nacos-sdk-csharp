namespace Nacos.Config.Remote.Grpc
{
    using Nacos.Remote;
    using System.Collections.Generic;

    public class ConfigRpcServerListFactory : IServerListFactory
    {
        private readonly IServerListFactory _serverListFactory;

        public ConfigRpcServerListFactory(IServerListFactory serverListFactory)
        {
            _serverListFactory = serverListFactory;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public string GenNextServer() => _serverListFactory.GenNextServer();

        public string GetCurrentServer() => _serverListFactory.GetCurrentServer();

        public string GetName()
        {
            throw new System.NotImplementedException();
        }

        public List<string> GetServerList() => _serverListFactory.GetServerList();
    }
}
