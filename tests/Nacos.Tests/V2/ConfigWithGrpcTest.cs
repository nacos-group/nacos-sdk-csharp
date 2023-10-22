namespace Nacos.Tests.V2
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Nacos;
    using Nacos.DependencyInjection;
    using System;
    using Xunit;
    using Xunit.Abstractions;

    [Trait("Category", "2x")]
    public class ConfigWithGrpcTest : ConfigBaseTest
    {
        public ConfigWithGrpcTest(ITestOutputHelper output)
        {
            _output = output;

            _output.WriteLine($"{nameof(ConfigWithGrpcTest)} BuildServiceProvider");
            _configSvc = BuildConfigService();
            _output.WriteLine($"{nameof(ConfigWithGrpcTest)} Get INacosConfigService");
        }

        protected override INacosConfigService BuildConfigService()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Config(x =>
            {
                x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs-test";

                /*x.UserName = "nacos";
                x.Password = "nacos";*/

                // swich to use http or rpc
                x.ConfigUseRpc = true;
            });

            services.AddLogging(builder => { builder.AddConsole(); });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<INacosConfigService>();
        }
    }
}
