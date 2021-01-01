namespace Nacos.Config.Abst
{
    using System.Collections.Generic;

    public interface IServerListManager
    {
        List<string> GetServerUrls();

        string GetCurrentServerAddr();

        string GetNextServerAddr();
    }
}
