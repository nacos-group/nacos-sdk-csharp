namespace Nacos.V2.Config.Abst
{
    using System.Threading.Tasks;

    public interface ILocalConfigInfoProcessor
    {
        Task<string> GetFailoverAsync(string serverName, string dataId, string group, string tenant);

        Task<string> GetSnapshotAync(string name, string dataId, string group, string tenant);

        Task SaveSnapshotAsync(string envName, string dataId, string group, string tenant, string config);
    }
}
