namespace Nacos.System.Configuration
{
    using global::Microsoft.Configuration.ConfigurationBuilders;
    using global::Microsoft.Extensions.Logging;
    using global::Microsoft.Extensions.Logging.Abstractions;
    using global::System;
    using global::System.Collections.Concurrent;
    using global::System.Collections.Generic;
    using global::System.Collections.Specialized;
    using global::System.Configuration;
    using global::System.Linq;
    using global::System.Reflection;
    using global::System.Threading.Tasks;
    using Nacos.Microsoft.Extensions.Configuration;

    public class NacosConfigurationBuilder : KeyValueConfigBuilder
    {
        public static ILoggerFactory LoggerFactory { get; set; }

        private static readonly FieldInfo ConfigurationManagerReset = typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly Dictionary<string, (NacosConfigurationSection, INacosConfigClient)> ClientCache = new Dictionary<string, (NacosConfigurationSection, INacosConfigClient)>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, string> ConfigCache = new ConcurrentDictionary<string, string>();

        private Task<IDictionary<string, string>[]> _data;

        public override void Initialize(string name, NameValueCollection config)
        {
            config ??= new NameValueCollection();

            if (config["mode"] == null)
            {
                config["mode"] = nameof(KeyValueMode.Greedy);
            }

            base.Initialize(name, config);
        }

        protected override void LazyInitialize(string name, NameValueCollection config)
        {
            base.LazyInitialize(name, config);

            var sectionName = string.IsNullOrWhiteSpace(config["nacosConfig"]) ? "nacosConfig" : config["nacosConfig"];
            if (!ClientCache.TryGetValue(sectionName, out var cache))
            {
                lock (ClientCache)
                {
                    if (!ClientCache.TryGetValue(sectionName, out cache))
                    {
                        var configurationSource = NacosConfigurationSection.GetConfig(sectionName);

                        var client = new NacosMsConfigClient(LoggerFactory ?? NullLoggerFactory.Instance, new NacosOptions
                        {
                            ServerAddresses = configurationSource.ServerAddresses.Split(';', ',').ToList(),
                            Namespace = configurationSource.Tenant,
                            AccessKey = configurationSource.AccessKey,
                            ClusterName = configurationSource.ClusterName,
                            ContextPath = configurationSource.ContextPath,
                            EndPoint = configurationSource.EndPoint,
                            DefaultTimeOut = configurationSource.DefaultTimeOut,
                            SecretKey = configurationSource.SecretKey,
                            Password = configurationSource.Password,
                            UserName = configurationSource.UserName,
                            ListenInterval = 20000
                        });

                        if (configurationSource.Listeners != null && configurationSource.Listeners.Count > 0)
                        {
                            _ = Task.WhenAll(configurationSource.Listeners
                                  .OfType<ConfigListener>()
                                  .Select(item => client.AddListenerAsync(new AddListenerRequest
                                  {
                                      DataId = item.DataId,
                                      Group = item.Group,
                                      Tenant = configurationSource.Tenant,
                                      Callbacks = new List<Action<string>> { x => CallBackReload($"{configurationSource.Tenant}#{item.Group}#{item.DataId}", x) }
                                  })).ToArray());
                        }

                        ClientCache[sectionName] = cache = (configurationSource, client);
                    }
                }
            }

            _data = GetConfig(cache.Item1, cache.Item2);
        }

        private static Task<IDictionary<string, string>[]> GetConfig(NacosConfigurationSection configurationSource, INacosConfigClient client) =>
            Task.WhenAll(configurationSource.Listeners
                .OfType<ConfigListener>()
                .Select(async item => item.NacosConfigurationParser.Parse(
                    ConfigCache.TryGetValue($"{configurationSource.Tenant}#{item.Group}#{item.DataId}", out var data)
                        ? data
                        : await client.GetConfigAsync(new GetConfigRequest
                        {
                            DataId = item.DataId,
                            Group = item.Group,
                            Tenant = configurationSource.Tenant
                        }).ConfigureAwait(false))));

        private static void CallBackReload(string key, string data)
        {
            ConfigCache[key] = data;

            try
            {
                ConfigurationManagerReset.SetValue(null, 0);
            }
            catch
            {
                // ignored
            }
        }

        public override string GetValue(string key)
        {
            foreach (var dic in _data.GetAwaiter().GetResult())
            {
                if (dic.TryGetValue(key, out var value)) return value;
            }

            return null;
        }

        public override ICollection<KeyValuePair<string, string>> GetAllValues(string prefix) =>
            _data.GetAwaiter().GetResult().SelectMany(dic => dic).ToArray();
    }
}