namespace Nacos.Tests.V2
{
    using Microsoft.Extensions.DependencyInjection;
    using Nacos.V2;
    using Nacos.V2.DependencyInjection;
    using System;
    using Xunit.Abstractions;

    public class ConfigWithHttpTest : ConfigBaseTest
    {
        public ConfigWithHttpTest(ITestOutputHelper output)
        {
            _output = output;

            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Config(x =>
            {
                x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs-test";

                // swich to use http or rpc
                x.ConfigUseRpc = false;
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _configSvc = serviceProvider.GetService<INacosConfigService>();
        }
    }
}
