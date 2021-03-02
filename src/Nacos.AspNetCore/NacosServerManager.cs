namespace Nacos.AspNetCore
{
    using EasyCaching.Core;
    using Microsoft.Extensions.Options;
    using Nacos;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Obsolete("This interface is obsolete and will be removed in a future version.")]
    public class NacosServerManager : INacosServerManager
    {
        private readonly INacosNamingClient _client;
        private readonly IEasyCachingProvider _provider;
        private readonly ILBStrategy _strategy;

        public NacosServerManager(
            INacosNamingClient client,
            IEasyCachingProviderFactory factory,
            IEnumerable<ILBStrategy> strategies,
            IOptions<NacosAspNetCoreOptions> optionsAccs)
        {
            _client = client;
            _provider = factory.GetCachingProvider("nacos.aspnetcore");
            _strategy = strategies.FirstOrDefault(x => x.Name.ToString().Equals(optionsAccs.Value.LBStrategy, StringComparison.OrdinalIgnoreCase))
                ?? new WeightRandomLBStrategy();
        }

        public async Task<string> GetServerAsync(string serviceName)
        {
            return await GetUrlAsync(serviceName, null, null, null);
        }

        public async Task<string> GetServerAsync(string serviceName, string groupName)
        {
            return await GetUrlAsync(serviceName, groupName, null, null);
        }

        public async Task<string> GetServerAsync(string serviceName, string groupName, string clusters)
        {
            return await GetUrlAsync(serviceName, groupName, clusters, null);
        }

        public async Task<string> GetServerAsync(string serviceName, string groupName, string clusters, string namespaceId)
        {
            return await GetUrlAsync(serviceName, groupName, clusters, namespaceId);
        }

        public async Task<List<Host>> GetServerListAsync(string serviceName)
        {
            return await GetServerListInnerAsync(serviceName, null, null, null);
        }

        public async Task<List<Host>> GetServerListAsync(string serviceName, string groupName)
        {
            return await GetServerListInnerAsync(serviceName, groupName, null, null);
        }

        public async Task<List<Host>> GetServerListAsync(string serviceName, string groupName, string clusters)
        {
            return await GetServerListInnerAsync(serviceName, groupName, clusters, null);
        }

        public async Task<List<Host>> GetServerListAsync(string serviceName, string groupName, string clusters, string namespaceId)
        {
            return await GetServerListInnerAsync(serviceName, groupName, clusters, namespaceId);
        }

        public async Task<Host> GetServerInfoAsync(string serviceName)
        {
            return await GetHostAsync(serviceName, null, null, null);
        }

        public async Task<Host> GetServerInfoAsync(string serviceName, string groupName)
        {
            return await GetHostAsync(serviceName, groupName, null, null);
        }

        public async Task<Host> GetServerInfoAsync(string serviceName, string groupName, string clusters)
        {
            return await GetHostAsync(serviceName, groupName, clusters, null);
        }

        public async Task<Host> GetServerInfoAsync(string serviceName, string groupName, string clusters, string namespaceId)
        {
            return await GetHostAsync(serviceName, groupName, clusters, namespaceId);
        }

        private async Task<List<Host>> GetServerListInnerAsync(string serviceName, string groupName, string clusters, string namespaceId)
        {
            var cachedKey = $"{serviceName}-{groupName}-{clusters}-{namespaceId}";

            var cached = await _provider.GetAsync(cachedKey, async () =>
            {
                var serviceInstances = await _client.ListInstancesAsync(new ListInstancesRequest
                {
                    ServiceName = serviceName,
                    GroupName = groupName,
                    Clusters = clusters,
                    NamespaceId = namespaceId,
                    HealthyOnly = true,
                });

                if (serviceInstances?.Hosts == null || !serviceInstances.Hosts.Any())
                    return null;
                return serviceInstances.Hosts.ToList();
            }, TimeSpan.FromSeconds(8));

            return cached.HasValue ? cached.Value : null;
        }

        private async Task<string> GetUrlAsync(string serviceName, string groupName, string clusters, string namespaceId)
        {
            var list = await GetServerListInnerAsync(serviceName, groupName, clusters, namespaceId);

            if (list != null && list.Any())
            {
                var host = _strategy.GetHost(list);

                // it seems that nacos don't return the scheme
                // so here use http only.
                return $"http://{host.Ip}:{host.Port}";
            }

            return null;
        }

        private async Task<Host> GetHostAsync(string serviceName, string groupName, string clusters, string namespaceId)
        {
            var list = await GetServerListInnerAsync(serviceName, groupName, clusters, namespaceId);

            if (list != null && list.Any())
            {
                var host = _strategy.GetHost(list);
                return host;
            }

            return null;
        }
    }
}
