namespace Nacos.Microsoft.Extensions.Configuration
{
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.Logging.Abstractions;
    using Nacos.Config;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class NacosConfigurationProvider : ConfigurationProvider
    {
        private readonly NacosConfigurationSource _configurationSource;

        private readonly INacosConfigurationParser _parser;

        private readonly INacosConfigClient _client;

        private readonly ConcurrentDictionary<string, string> _configDict;

        public NacosConfigurationProvider(NacosConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;
            _parser = configurationSource.NacosConfigurationParser;
            _configDict = new ConcurrentDictionary<string, string>();

            _client = new NacosMsConfigClient(NullLoggerFactory.Instance, new NacosOptions
            {
                ServerAddresses = configurationSource.ServerAddresses,
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

            if (configurationSource.Listeners != null && configurationSource.Listeners.Any())
            {
                var tasks = new List<Task>();

                foreach (var item in configurationSource.Listeners)
                {
                    tasks.Add(_client.AddListenerAsync(new AddListenerRequest
                    {
                        DataId = item.DataId,
                        Group = item.Group,
                        Tenant = configurationSource.Tenant,
                        Callbacks = new System.Collections.Generic.List<Action<string>>
                        {
                            x => CallBackReload($"{configurationSource.Tenant}#{item.Group}#{item.DataId}", x, item.Optional)
                        }
                    }));
                }

                Task.WhenAll(tasks).ConfigureAwait(false);
            }
            else
            {
                _client.AddListenerAsync(new AddListenerRequest
                {
#pragma warning disable CS0618
                    DataId = _configurationSource.DataId,
                    Group = _configurationSource.Group,
                    Tenant = _configurationSource.Tenant,
                    Callbacks = new System.Collections.Generic.List<Action<string>>
                    {
                        x => CallBackReload($"{_configurationSource.Tenant}#{_configurationSource.Group}#{_configurationSource.DataId}", x, _configurationSource.Optional)
                    }
#pragma warning restore CS0618
                });
            }
        }

        private void CallBackReload(string key, string val, bool optional)
        {
            try
            {
                _configDict[key] = val;

                var nData = new Dictionary<string, string>();

                foreach (var dict in _configDict)
                {
                    var data = _parser.Parse(dict.Value);

                    foreach (var item in data)
                    {
                        nData.Add(item.Key, item.Value);
                    }
                }

                Data = nData;
                OnReload();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"call back reload config error, {ex.Message}");
                if (!optional)
                {
                    throw;
                }
            }
        }

        public override void Load()
        {
            try
            {
                if (_configurationSource.Listeners != null && _configurationSource.Listeners.Any())
                {
                    var dict = new Dictionary<string, string>();

                    foreach (var listener in _configurationSource.Listeners)
                    {
                        try
                        {
                            var config = _client.GetConfigAsync(new GetConfigRequest
                            {
                                DataId = listener.DataId,
                                Group = listener.Group,
                                Tenant = _configurationSource.Tenant
                            }).ConfigureAwait(false).GetAwaiter().GetResult();

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
                        var config = _client.GetConfigAsync(new GetConfigRequest
                        {
                            DataId = _configurationSource.DataId,
                            Group = _configurationSource.Group,
                            Tenant = _configurationSource.Tenant
                        }).ConfigureAwait(false).GetAwaiter().GetResult();

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
    }
}
