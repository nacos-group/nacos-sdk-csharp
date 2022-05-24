namespace Nacos.Microsoft.Extensions.Configuration
{
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.Logging;
    using global::Microsoft.Extensions.Options;
    using Nacos.V2;
    using Nacos.V2.Config;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class NacosV2ConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private readonly NacosV2ConfigurationSource _configurationSource;

        private readonly INacosConfigurationParser _parser;

        private readonly INacosConfigService _client;

        private readonly ConcurrentDictionary<string, string> _configDict;

        private readonly Dictionary<string, MsConfigListener> _listenerDict;

        private readonly ILogger _logger;

        public NacosV2ConfigurationProvider(NacosV2ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
            _parser = configurationSource.NacosConfigurationParser;
            _configDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _listenerDict = new Dictionary<string, MsConfigListener>();

            var options = Options.Create(new NacosSdkOptions()
            {
                ServerAddresses = configurationSource.ServerAddresses,
                Namespace = configurationSource.GetNamespace(),
                AccessKey = configurationSource.AccessKey,
                ContextPath = configurationSource.ContextPath,
                EndPoint = configurationSource.EndPoint,
                DefaultTimeOut = configurationSource.DefaultTimeOut,
                SecretKey = configurationSource.SecretKey,
                Password = configurationSource.Password,
                UserName = configurationSource.UserName,
                ListenInterval = 20000,
                ConfigUseRpc = configurationSource.ConfigUseRpc,
                ConfigFilterAssemblies = configurationSource.ConfigFilterAssemblies,
                ConfigFilterExtInfo = configurationSource.ConfigFilterExtInfo,
            });

            var nacosLoggerFactory = Nacos.Microsoft.Extensions.Configuration.NacosLog.NacosLoggerFactory.GetInstance(configurationSource.LoggingBuilder);
            _logger = nacosLoggerFactory.CreateLogger<NacosV2ConfigurationProvider>();
            _client = new NacosConfigService(nacosLoggerFactory, options);
            if (configurationSource.Listeners != null && configurationSource.Listeners.Any())
            {
                var tasks = new List<Task>();

                foreach (var item in configurationSource.Listeners)
                {
                    var listener = new MsConfigListener(item.DataId, item.Group, item.Optional, this, _logger);

                    tasks.Add(_client.AddListener(item.DataId, item.Group, listener));

                    _listenerDict.Add($"{item.DataId}#{item.Group}", listener);
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                // after remove old v1 code, Listeners must be not empty
                throw new Nacos.V2.Exceptions.NacosException("Listeners is empty!!");
            }
        }

        // for test
        internal NacosV2ConfigurationProvider(NacosV2ConfigurationSource configurationSource, ILogger logger, INacosConfigService client, INacosConfigurationParser parser)
        {
            _configurationSource = configurationSource;
            _parser = parser;
            _configDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _listenerDict = new Dictionary<string, MsConfigListener>();
            _logger = logger;
            _client = client;
            if (configurationSource.Listeners != null && configurationSource.Listeners.Any())
            {
                var tasks = new List<Task>();

                foreach (var item in configurationSource.Listeners)
                {
                    var listener = new MsConfigListener(item.DataId, item.Group, item.Optional, this, _logger);

                    tasks.Add(_client.AddListener(item.DataId, item.Group, listener));

                    _listenerDict.Add($"{item.DataId}#{item.Group}", listener);
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                // after remove old v1 code, Listeners must be not empty
                throw new Nacos.V2.Exceptions.NacosException("Listeners is empty!!");
            }
        }

        public void Dispose()
        {
            var tasks = new List<Task>();

            foreach (var item in _listenerDict)
            {
                var arr = item.Key.Split('#');
                var dataId = arr[0];
                var group = arr[1];

                tasks.Add(_client.RemoveListener(dataId, group, item.Value));
            }

            Task.WaitAll(tasks.ToArray());

            _logger?.LogInformation($"Remove All Listeners");
        }

        public override void Load()
        {
            try
            {
                if (_configurationSource.Listeners != null && _configurationSource.Listeners.Any())
                {
                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var listener in _configurationSource.Listeners)
                    {
                        try
                        {
                            var config = _client.GetConfig(listener.DataId, listener.Group, 3000)
                                .ConfigureAwait(false).GetAwaiter().GetResult();

                            _configDict.AddOrUpdate($"{_configurationSource.GetNamespace()}#{listener.Group}#{listener.DataId}", config, (x, y) => config);

                            var data = _parser.Parse(config);

                            foreach (var item in data)
                            {
                                dict[item.Key] = item.Value;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "MS Config Query config error, dataid={0}, group={1}, tenant={2}", listener.DataId, listener.Group, listener.Tenant);
                            if (!listener.Optional)
                            {
                                throw;
                            }
                        }
                    }

                    Data = dict;
                }
                else
                {
                    // after remove old v1 code, Listeners must be not empty
                    throw new Nacos.V2.Exceptions.NacosException("Listeners is empty!!");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Load config error");
            }
        }

        // for test
        internal void SetListener(string key, MsConfigListener listener)
        {
            _listenerDict[key] = listener;
        }

        internal class MsConfigListener : IListener
        {
            private string _dataId;
            private string _group;
            private bool _optional;
            private NacosV2ConfigurationProvider _provider;
            private string _key;
            private ILogger _logger;

            internal MsConfigListener(string dataId, string group, bool optional, NacosV2ConfigurationProvider provider, ILogger logger)
            {
                this._dataId = dataId;
                this._group = group;
                this._optional = optional;
                this._provider = provider;
                this._logger = logger;
                _key = $"{provider._configurationSource.GetNamespace()}#{_group}#{_dataId}";
            }


            public void ReceiveConfigInfo(string configInfo)
            {
                _logger?.LogDebug("MsConfigListener Receive ConfigInfo 【{0}】", configInfo);
                try
                {
                    _provider._configDict.AddOrUpdate(_key, configInfo, (x, y) => configInfo);

                    var nData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var listener in _provider._configurationSource.Listeners)
                    {
                        var key = $"{_provider._configurationSource.GetNamespace()}#{listener.Group}#{listener.DataId}";

                        if (!_provider._configDict.TryGetValue(key, out var config))
                        {
                            continue;
                        }

                        var data = _provider._parser.Parse(config);

                        foreach (var item in data)
                        {
                            nData[item.Key] = item.Value;
                        }
                    }

                    _provider.Data = nData;
                    _provider.OnReload();
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, $"call back reload config error");
                    if (!_optional)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
