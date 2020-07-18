namespace Nacos.AspNetCore
{
    using System.Threading.Tasks;

    public interface INacosServerManager
    {
        Task<string> GetServerAsync(string serviceName);
    }
}
