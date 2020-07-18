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
            var cached = await _provider.GetAsync(serviceName, async () =>
            {
                var serviceInstances = await _client.ListInstancesAsync(new ListInstancesRequest
                {
                    ServiceName = serviceName,
                    HealthyOnly = true,
                });

                var baseUrl = string.Empty;

                if (serviceInstances != null && serviceInstances.Hosts != null && serviceInstances.Hosts.Any())
                {
                    var list = serviceInstances.Hosts.Select(x => new NacosServer
                    {
                        // it seems that nacos don't return the scheme
                        // so here use http only.
                        Url = $"http://{x.Ip}:{x.Port}"
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
    }
}
