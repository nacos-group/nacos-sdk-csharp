namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Config.Http;
    using System.Collections.Generic;

    public class NacosConfigClient : AbstNacosConfigClient
    {
        private readonly IHttpAgent _httpAgent;
        private readonly ILocalConfigInfoProcessor _processor;

        public NacosConfigClient(
           ILoggerFactory loggerFactory,
           IOptionsMonitor<NacosOptions> optionAccs,
           IHttpAgent httpAgent,
           ILocalConfigInfoProcessor processor)
        {
            _logger = loggerFactory.CreateLogger<NacosConfigClient>();
            _options = optionAccs.CurrentValue;
            _httpAgent = httpAgent;
            _processor = processor;

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
