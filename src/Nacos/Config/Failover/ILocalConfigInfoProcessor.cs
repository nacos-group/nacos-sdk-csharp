namespace Nacos
{
    using System.Threading.Tasks;

    public interface ILocalConfigInfoProcessor
    {
        Task<string> GetFailoverAsync(string dataId, string group, string tenant);

        Task<string> GetSnapshotAync(string dataId, string group, string tenant);

        Task SaveSnapshotAsync(string dataId, string group, string tenant, string config);
    }
}
