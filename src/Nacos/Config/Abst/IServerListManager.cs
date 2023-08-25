namespace Nacos.Config.Abst
{
    using System;
    using System.Collections.Generic;

    public interface IServerListManager : IDisposable
    {
        List<string> GetServerUrls();

        string GetCurrentServerAddr();

        void RefreshCurrentServerAddr();

        void UpdateCurrentServerAddr(string currentServerAddr);

        string GetName();

        string GetNamespace();

        string GetTenant();

        string GetContentPath();

        string GetNextServerAddr();
    }
}
