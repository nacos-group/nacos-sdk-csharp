namespace Nacos.Tests.V2
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Nacos.V2;
    using Nacos.V2.DependencyInjection;
    using Nacos.V2.Utils;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    [Trait("Category", "auth")]
    public class AuthTest
    {
        protected INacosNamingService _namingSvc;
        protected INacosConfigService _configSvc;

        protected ITestOutputHelper _output;

        public AuthTest(ITestOutputHelper output)
        {
            _output = output;

            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Config(x =>
            {
                x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs-test";

                x.UserName = "nacos";
                x.Password = "nacos";

                // swich to use http or rpc
                x.ConfigUseRpc = true;
                x.NamingUseRpc = true;
            });

            services.AddNacosV2Naming(x =>
            {
            });

            services.AddLogging(builder => { builder.AddConsole(); });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _configSvc = serviceProvider.GetService<INacosConfigService>();
            _namingSvc = serviceProvider.GetService<INacosNamingService>();
        }

        [Fact]
        protected virtual async Task Naming_Should_Succeed()
        {
            var serviceName = $"auth-{Guid.NewGuid().ToString()}";
            var instances = await _namingSvc.GetAllInstances(serviceName, false);
            _output.WriteLine($"Naming_Should_Succeed, GetAllInstances, {serviceName}, {instances?.ToJsonString()}");
            Assert.Empty(instances);
        }

        [Fact]
        protected virtual async Task Config_Should_Succeed()
        {
            var dataId = $"get-{Guid.NewGuid().ToString()}";
            var group = Nacos.V2.Common.Constants.DEFAULT_GROUP;

            var config = await _configSvc.GetConfig(dataId, group, 10000L);
            _output.WriteLine($"Config_Should_Succeed, GetConfig {dataId} return {config}");
            Assert.Null(config);
        }
    }
}
