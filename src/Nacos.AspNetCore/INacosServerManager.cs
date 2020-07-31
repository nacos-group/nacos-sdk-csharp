namespace Nacos.AspNetCore
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface INacosServerManager
    {
        Task<string> GetServerAsync(string serviceName);

        Task<string> GetServerAsync(string serviceName, string groupName);

        Task<string> GetServerAsync(string serviceName, string groupName, string clusters);

        Task<string> GetServerAsync(string serviceName, string groupName, string clusters, string namespaceId);

        Task<List<Host>> GetServerListAsync(string serviceName);

        Task<List<Host>> GetServerListAsync(string serviceName, string groupName);

        Task<List<Host>> GetServerListAsync(string serviceName, string groupName, string clusters);

        Task<List<Host>> GetServerListAsync(string serviceName, string groupName, string clusters, string namespaceId);
    }
}
