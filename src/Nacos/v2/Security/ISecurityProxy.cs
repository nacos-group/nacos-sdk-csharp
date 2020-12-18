namespace Nacos.Security.V2
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISecurityProxy
    {
        Task<bool> LoginAsync(List<string> servers);

        string GetAccessToken();
    }
}
