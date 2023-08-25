namespace Nacos.Remote
{
    using System.Collections.Generic;

    public interface IServerListFactory
    {
        string GenNextServer();

        string GetCurrentServer();

        List<string> GetServerList();
    }
}
