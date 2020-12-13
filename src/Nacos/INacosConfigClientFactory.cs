namespace Nacos
{
    using System.Linq;

    public interface INacosConfigClientFactory
    {
        INacosConfigClient GetConfigClient(string name);
    }

    public class NacosConfigClientFactory : INacosConfigClientFactory
    {
        private readonly System.Collections.Generic.IEnumerable<INacosConfigClient> _clients;

        public NacosConfigClientFactory(System.Collections.Generic.IEnumerable<INacosConfigClient> clients)
        {
            _clients = clients;
        }

        public INacosConfigClient GetConfigClient(string name)
        {
            return _clients.First(x => x.Name.Equals(name));
        }
    }
}
