namespace Nacos.Tests.V2
{
    using Microsoft.Extensions.DependencyInjection;
    using Nacos.V2;
    using Nacos.V2.DependencyInjection;
    using System;
    using Xunit.Abstractions;

    public class NamingWithHttpTest : NamingBaseTest
    {
        public NamingWithHttpTest(ITestOutputHelper output)
        {
            _output = output;

            IServiceCollection services = new ServiceCollection();

            services.AddNacosV2Naming(x =>
            {
                x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
                x.EndPoint = "";
                x.Namespace = "cs-test";

                // swich to use http or rpc
                x.NamingUseRpc = false;
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _namingSvc = serviceProvider.GetService<INacosNamingService>();
        }
    }
}
