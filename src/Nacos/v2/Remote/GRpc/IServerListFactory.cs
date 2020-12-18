namespace Nacos.Remote.GRpc
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServerListFactory
    {
        Task<string> GenNextServerAsync();

        Task<string> GetCurrentServerAsync();

        List<string> GetServerListAsync();
    }
}
