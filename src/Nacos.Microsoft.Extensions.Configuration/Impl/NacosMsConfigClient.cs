namespace Nacos.Microsoft.Extensions.Configuration
{
    using global::Microsoft.Extensions.Logging;
    using global::System.Collections.Generic;
    using Nacos;
    using Nacos.Config.Http;

    public class NacosMsConfigClient : AbstNacosConfigClient
    {
        private readonly Nacos.Config.Http.IHttpAgent _httpAgent;
        private readonly ILocalConfigInfoProcessor _processor;

        public NacosMsConfigClient(
            ILoggerFactory loggerFactory,
            NacosOptions options)
        {
            _logger = loggerFactory.CreateLogger<NacosMsConfigClient>();
            _options = options;
            _processor = new FileLocalConfigInfoProcessor();

            _httpAgent = new MsConfigServerHttpAgent(_options);

            listeners = new List<Listener>();
        }

        public override IHttpAgent GetAgent()
        {
            return _httpAgent;
        }

        public override ILocalConfigInfoProcessor GetProcessor()
        {
            return _processor;
        }
    }
}
