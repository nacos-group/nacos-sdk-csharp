namespace Nacos.System.Configuration
{
    using global::Microsoft.Configuration.ConfigurationBuilders;
    using global::Microsoft.Extensions.Logging;
    using global::Microsoft.Extensions.Logging.Abstractions;
    using global::Microsoft.Extensions.Options;
    using global::System;
    using global::System.Collections.Concurrent;
    using global::System.Collections.Generic;
    using global::System.Collections.Specialized;
    using global::System.Configuration;
    using global::System.Diagnostics;
    using global::System.Linq;
    using global::System.Reflection;
    using global::System.Threading.Tasks;
    using Nacos.V2;
    using Nacos.V2.Config;

    public class NacosConfigurationBuilder : KeyValueConfigBuilder
    {
        public static ILoggerFactory LoggerFactory { get; set; }

        private static readonly FieldInfo ConfigurationManagerReset = typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static)!;
        private static readonly Dictionary<string, Tuple<NacosConfigurationSection, INacosConfigService>> ClientCache = new Dictionary<string, Tuple<NacosConfigurationSection, INacosConfigService>>(StringComparer.OrdinalIgnoreCase);
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

            var sectionName = string.IsNullOrWhiteSpace(config["nacosConfig"]) ? "nacos" : config["nacosConfig"];
            if (!ClientCache.TryGetValue(sectionName, out var cache))
            {
                lock (ClientCache)
                {
                    if (!ClientCache.TryGetValue(sectionName, out cache))
                    {
                        var nacosConfig = NacosConfigurationSection.GetConfig(sectionName);
                        if (nacosConfig == null)
                        {
                            LoggerFactory?.CreateLogger<NacosConfigurationBuilder>().LogWarning($"Can't found `{sectionName}` config");

                            Trace.TraceWarning($"Can't found `{sectionName}` config");

                            ClientCache[sectionName] = null;

                            return;
                        }

                        INacosConfigService client = new NacosConfigService(LoggerFactory ?? NullLoggerFactory.Instance, Options.Create(new NacosSdkOptions
                        {
                            ServerAddresses = nacosConfig.ServerAddresses.Split(';', ',').ToList(),
                            Namespace = nacosConfig.Tenant,
                            AccessKey = nacosConfig.AccessKey,
                            ContextPath = nacosConfig.ContextPath,
                            EndPoint = nacosConfig.EndPoint,
                            DefaultTimeOut = nacosConfig.DefaultTimeOut,
                            SecretKey = nacosConfig.SecretKey,
                            Password = nacosConfig.Password,
                            UserName = nacosConfig.UserName,
                            ListenInterval = 20000,
                            ConfigUseRpc = nacosConfig.UseGrpc,
                        }));

                        ClientCache[sectionName] = cache = Tuple.Create(nacosConfig, client);

                        if (nacosConfig.Listeners != null && nacosConfig.Listeners.Count > 0)
                        {
                            try
                            {
                                _ = Task.WhenAll(nacosConfig.Listeners
                                    .OfType<ConfigListener>()
                                    .Select(item => cache.Item2.AddListener(item.DataId, item.Group ?? Nacos.V2.Common.Constants.DEFAULT_GROUP, new MsConfigListener($"{nacosConfig.Tenant}#{item.Group}#{item.DataId}"))));
                            }
                            catch (Exception ex)
                            {
                                LoggerFactory?.CreateLogger<NacosConfigurationBuilder>().LogError(ex, "AddListener fail.");

                                Trace.TraceError("AddListener fail" + Environment.NewLine + ex);
                            }
                        }
                    }
                }
            }

            _data = cache == null ? Task.FromResult(Array.Empty<IDictionary<string, string>>()) : GetConfig(cache.Item1, cache.Item2);
        }

        private static Task<IDictionary<string, string>[]> GetConfig(NacosConfigurationSection config, INacosConfigService client) =>
            Task.WhenAll(config.Listeners.OfType<ConfigListener>()
                .Select(async item =>
                {
                    if (!ConfigCache.TryGetValue($"{config.Tenant}#{item.Group}#{item.DataId}", out var data))
                    {
                        try
                        {
                            data = await client.GetConfig(item.DataId, item.Group ?? Nacos.V2.Common.Constants.DEFAULT_GROUP, 3000)
                                .ConfigureAwait(false);
                            if (data == null)
                            {
                                LoggerFactory?.CreateLogger<NacosConfigurationBuilder>().LogWarning($"Can't get config {item.Group}#{item.DataId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerFactory?.CreateLogger<NacosConfigurationBuilder>().LogError(ex, $"GetConfig({item.Group}#{item.DataId}) fail.");

                            Trace.TraceError($"GetConfig({item.Group}#{item.DataId}) fail" + Environment.NewLine + ex);
                        }
                    }

                    return data == null ? new Dictionary<string, string>() : item.NacosConfigurationParser.Parse(data);
                }));

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

        private class MsConfigListener : IListener
        {
            private readonly string _key;

            public MsConfigListener(string key) => _key = key;

            public void ReceiveConfigInfo(string configInfo) => CallBackReload(_key, configInfo);
        }
    }
}
