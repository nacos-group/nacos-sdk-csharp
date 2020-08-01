namespace Nacos.AspNetCore
{
    using EasyCaching.Core;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
            return await GetServerInnerAsync(serviceName, null, null, null);
        }

        public async Task<string> GetServerAsync(string serviceName, string groupName)
        {
            return await GetServerInnerAsync(serviceName, groupName, null, null);
        }

        public async Task<string> GetServerAsync(string serviceName, string groupName, string clusters)
        {
            return await GetServerInnerAsync(serviceName, groupName, clusters, null);
        }

        public async Task<string> GetServerAsync(string serviceName, string groupName, string clusters, string namespaceId)
        {
            return await GetServerInnerAsync(serviceName, groupName, clusters, namespaceId);
        }

        private async Task<string> GetServerInnerAsync(string serviceName, string groupName, string clusters, string namespaceId)
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

                if (serviceInstances != null && serviceInstances.Hosts != null && serviceInstances.Hosts.Any())
                {
                    var list = serviceInstances.Hosts.Select(x => new NacosServer
                    {
                        // it seems that nacos don't return the scheme
                        // so here use http only.
                        Url = $"http://{x.Ip}:{x.Port}",
                        Weight = x.Weight
                    }).ToList();

                    return list;
                }

                return null;
            }, TimeSpan.FromSeconds(10));

            if (cached.HasValue)
            {
                var list = cached.Value;
                var instance = _strategy.GetInstance(list);
                return instance;
            }
            else
            {
                return null;
            }
        }



        /// <summary>
        /// Get the Host list of the service named serviceName
        /// </summary>
        /// <param name="serviceName">ServiceName</param>
        /// <returns>Host list</returns>
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
            }, TimeSpan.FromSeconds(10));
            return cached.HasValue ? cached.Value : null;
        }
    }
}
