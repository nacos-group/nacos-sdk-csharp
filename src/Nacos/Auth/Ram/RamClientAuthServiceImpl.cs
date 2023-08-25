namespace Nacos.Auth.Ram
{
    using Nacos.Auth.Ram.Injector;
    using Nacos.Utils;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RamClientAuthServiceImpl : IClientAuthService
    {
        private readonly RamContext _ramContext;

        private readonly Dictionary<string, AbstractResourceInjector> _resourceInjectors;

        private List<string> _serverList;

        public RamClientAuthServiceImpl()
        {
            _ramContext = new RamContext();
            _resourceInjectors = new Dictionary<string, AbstractResourceInjector>
            {
                { NacosAuthLoginConstant.NAMING, new NamingResourceInjector() },
                { NacosAuthLoginConstant.CONFIG, new ConfigResourceInjector() }
            };
        }

        // For UT only
        internal RamClientAuthServiceImpl(Dictionary<string, AbstractResourceInjector> testResourceInjectors)
        {
            _ramContext = new RamContext();
            _resourceInjectors = testResourceInjectors;
        }

        internal RamContext GetRamContext() => _ramContext;

        public LoginIdentityContext GetLoginIdentityContext(RequestResource resource)
        {
            LoginIdentityContext result = new();
            if (!_ramContext.Validate() || NotFountInjector(resource.Type))
            {
                return result;
            }

            _resourceInjectors[resource.Type].DoInject(resource, _ramContext, result);
            return result;
        }

        private bool NotFountInjector(string type)
        {
            if (!_resourceInjectors.ContainsKey(type))
            {
                // LOGGER.warn("Injector for type {} not found, will use default ram identity context.", type);
                return true;
            }

            return false;
        }

        public Task<bool> Login(NacosSdkOptions options)
        {
            if (_ramContext.Validate()) return Task.FromResult(true);

            if (options.RamRoleName.IsNotNullOrWhiteSpace())
            {
                // TODO: STS
                _ramContext.RamRoleName = options.RamRoleName;
            }

            _ramContext.AccessKey = options.AccessKey.IsNullOrWhiteSpace()
                ? "" // SpasAdapter.getAk()
                : options.AccessKey;

            _ramContext.SecretKey = options.SecretKey.IsNullOrWhiteSpace()
                ? "" // SpasAdapter.getSk()
                : options.SecretKey;

            return Task.FromResult(true);
        }

        public void SetServerList(List<string> serverList)
        {
            _serverList = serverList;
        }
    }
}
