namespace Nacos.Security
{
    using Nacos.Auth;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISecurityProxy
    {
        Task LoginAsync(List<string> servers);

        Dictionary<string, string> GetIdentityContext(RequestResource resource);
    }
}
