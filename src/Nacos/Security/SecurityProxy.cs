﻿namespace Nacos.Security
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos;
    using Nacos.Auth;
    using Nacos.Common;
    using Nacos.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SecurityProxy : ISecurityProxy
    {
        private readonly ILogger _logger = NacosLogManager.CreateLogger<SecurityProxy>();
        private readonly NacosSdkOptions _options;
        private readonly IEnumerable<IClientAuthService> _clientAuthServices;

        public SecurityProxy(NacosSdkOptions options)
        {
            _options = options;
            _clientAuthServices = null;
        }

        public SecurityProxy(IOptions<NacosSdkOptions> optionsAccs, IEnumerable<IClientAuthService> clientAuthServices)
        {
            _options = optionsAccs.Value;
            _clientAuthServices = clientAuthServices;
        }

        public async Task LoginAsync(List<string> servers)
        {
            if (_clientAuthServices == null || !_clientAuthServices.Any())
            {
                return;
            }

            foreach (var item in _clientAuthServices)
            {
                item.SetServerList(servers);
                await item.Login(_options).ConfigureAwait(false);
            }
        }

        public Dictionary<string, string> GetIdentityContext(RequestResource resource)
        {
            Dictionary<string, string> header = new(capacity: 1);
            foreach (var clientAuthService in _clientAuthServices)
            {
                var ctx = clientAuthService.GetLoginIdentityContext(resource);

                foreach (var key in ctx.GetAllKey())
                {
                    header[key] = ctx.GetParameter(key);
                }
            }

            return header;
        }
    }
}
