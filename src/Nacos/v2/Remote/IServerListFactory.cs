namespace Nacos.Remote
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServerListFactory
    {
        Task<string> GenNextServerAsync();

        Task<string> GetCurrentServerAsync();

        Task<List<string>> GetServerListAsync();
    }
}
