namespace Nacos.Microsoft.Extensions.Configuration
{
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.Logging.Abstractions;
    using System;

    internal class NacosConfigurationProvider : ConfigurationProvider
    {
        private readonly NacosConfigurationSource _configurationSource;

        private readonly INacosConfigurationParser _parser;

        private readonly INacosConfigClient _client;

        public NacosConfigurationProvider(NacosConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;

            _parser = configurationSource.NacosConfigurationParser ?? JsonConfigurationStringParser.Instance;

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
                ListenInterval = 5000
            });

            _client.AddListenerAsync(new AddListenerRequest
            {
                DataId = _configurationSource.DataId,
                Group = _configurationSource.Group,
                Tenant = _configurationSource.Tenant,
                Callbacks = new System.Collections.Generic.List<Action<string>>
                {
                    x => CallBackReload(x)
                }
            });
        }

        private void CallBackReload(string val)
        {
            try
            {
                var data = _parser.Parse(val);

                Data = data;

                OnReload();
            }
            catch
            {
                if (!_configurationSource.Optional)
                {
                    throw;
                }
            }
        }

        public override void Load()
        {
            try
            {
                var config = _client.GetConfigAsync(new GetConfigRequest
                {
                    DataId = _configurationSource.DataId,
                    Group = _configurationSource.Group,
                    Tenant = _configurationSource.Tenant
                }).ConfigureAwait(false).GetAwaiter().GetResult();

                var data = _parser.Parse(config);

                Data = data;
            }
            catch
            {
                if (!_configurationSource.Optional)
                {
                    throw;
                }
            }
        }
    }
}
