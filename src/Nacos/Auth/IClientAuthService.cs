namespace Nacos.Auth
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IClientAuthService
    {
        Task<bool> Login(NacosSdkOptions options);

        void SetServerList(List<string> serverList);

        LoginIdentityContext GetLoginIdentityContext(RequestResource resource);
    }
}
