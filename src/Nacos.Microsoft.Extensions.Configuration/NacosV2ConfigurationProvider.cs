namespace Nacos.Microsoft.Extensions.Configuration
{
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.Logging.Abstractions;
    using global::Microsoft.Extensions.Options;
    using Nacos.Config;
    using Nacos.V2;
    using Nacos.V2.Config;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class NacosV2ConfigurationProvider : ConfigurationProvider
    {
        private readonly NacosV2ConfigurationSource _configurationSource;

        private readonly INacosConfigurationParser _parser;

        private readonly INacosConfigService _client;

        private readonly ConcurrentDictionary<string, string> _configDict;

        public NacosV2ConfigurationProvider(NacosV2ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
            _parser = configurationSource.NacosConfigurationParser;
            _configDict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var options = Options.Create(new NacosSdkOptions()
            {
                ServerAddresses = configurationSource.ServerAddresses,
                Namespace = configurationSource.Tenant,
                AccessKey = configurationSource.AccessKey,
                ContextPath = configurationSource.ContextPath,
                EndPoint = configurationSource.EndPoint,
                DefaultTimeOut = configurationSource.DefaultTimeOut,
                SecretKey = configurationSource.SecretKey,
                Password = configurationSource.Password,
                UserName = configurationSource.UserName,
                ListenInterval = 20000,
                ConfigUseRpc = configurationSource.ConfigUseRpc
            });

            _client = new NacosConfigService(NullLoggerFactory.Instance, options);
            if (configurationSource.Listeners != null && configurationSource.Listeners.Any())
            {
                var tasks = new List<Task>();

                foreach (var item in configurationSource.Listeners)
                {
                    tasks.Add(_client.AddListener(item.DataId, item.Group, new MsConfigListener(item.DataId, item.Group, item.Optional, this)));
                }

                Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else
            {
#pragma warning disable CS0618 // 类型或成员已过时
                _client.AddListener(_configurationSource.DataId, _configurationSource.Group, new MsConfigListener(configurationSource.DataId, _configurationSource.Group, _configurationSource.Optional, this));
#pragma warning restore CS0618 // 类型或成员已过时
            }
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

                            _configDict.AddOrUpdate($"{_configurationSource.Tenant}#{listener.Group}#{listener.DataId}", config, (x, y) => config);

                            var data = _parser.Parse(config);

                            foreach (var item in data)
                            {
                                dict[item.Key] = item.Value;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine($"MS Config Query config error, {listener.DataId} ,{ex.Message}");
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
#pragma warning disable CS0618
                    try
                    {
                        var config = _client.GetConfig(_configurationSource.DataId, _configurationSource.Group, 3000)
                            .ConfigureAwait(false).GetAwaiter().GetResult();

                        _configDict.AddOrUpdate($"{_configurationSource.Tenant}#{_configurationSource.Group}#{_configurationSource.DataId}", config, (x, y) => config);

                        var data = _parser.Parse(config);

                        Data = data;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"MS Config Query config error, {_configurationSource.DataId} ,{ex.Message}");
                        if (!_configurationSource.Optional)
                        {
                            throw;
                        }
                    }
#pragma warning restore CS0618
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Load config error, {ex.Message}");
            }
        }

        internal class MsConfigListener : IListener
        {
            private string _dataId;
            private string _group;
            private bool _optional;
            private NacosV2ConfigurationProvider _provider;
            private string _key;

            internal MsConfigListener(string dataId, string group, bool optional, NacosV2ConfigurationProvider provider)
            {
                this._dataId = dataId;
                this._group = group;
                this._optional = optional;
                this._provider = provider;
                _key = $"{_dataId}#{_group}";
            }


            public void ReceiveConfigInfo(string configInfo)
            {
                try
                {
                    _provider._configDict[_key] = configInfo;

                    var nData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var dict in _provider._configDict)
                    {
                        var data = _provider._parser.Parse(dict.Value);

                        foreach (var item in data)
                        {
                            nData.Add(item.Key, item.Value);
                        }
                    }

                    _provider.Data = nData;
                    _provider.OnReload();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"call back reload config error, {ex.Message}");
                    if (!_optional)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
