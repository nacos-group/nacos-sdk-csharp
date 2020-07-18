namespace Nacos
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    public class MemoryLocalConfigInfoProcessor : ILocalConfigInfoProcessor
    {
        private readonly ConcurrentDictionary<string, string> _cache;

        public MemoryLocalConfigInfoProcessor()
        {
            _cache = new ConcurrentDictionary<string, string>();
        }

        private string GetCacheKey(string dataId, string group, string tenant)
        {
            return $"{tenant}-{group}-{dataId}";
        }

        public async Task<string> GetFailoverAsync(string serverName, string dataId, string group, string tenant)
        {
            var cacheKey = GetCacheKey(dataId, group, tenant);

            _cache.TryGetValue(cacheKey, out string config);

            return await Task.FromResult(config);
        }

        public async Task<string> GetSnapshotAync(string name, string dataId, string group, string tenant)
        {
            var cacheKey = GetCacheKey(dataId, group, tenant);

            _cache.TryGetValue(cacheKey, out string config);

            return await Task.FromResult(config);
        }

        public async Task SaveSnapshotAsync(string envName, string dataId, string group, string tenant, string config)
        {
            var cacheKey = GetCacheKey(dataId, group, tenant);

            if (string.IsNullOrEmpty(config))
            {
                _cache.TryRemove(cacheKey, out _);
            }
            else
            {
                _cache.AddOrUpdate(cacheKey, config, (k, v) => config);
            }

            await Task.Yield();
        }
    }
}
