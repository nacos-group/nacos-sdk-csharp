namespace Nacos.V2.Remote
{
    using System.Collections.Generic;

    public interface IServerListFactory
    {
        string GenNextServer();

        string GetCurrentServer();

        List<string> GetServerList();
    }
}
