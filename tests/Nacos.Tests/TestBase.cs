namespace Nacos.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using Nacos;
    using System;

    public class TestBase
    {
        protected INacosNamingClient _namingClient;
        protected INacosConfigClient _configClient;

        public TestBase()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddNacos(configure =>
            {
                configure.DefaultTimeOut = 8000;
                configure.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848", };
                configure.AccessKey = "";
                configure.SecretKey = "";

                // configure.Namespace = "";
                configure.Namespace = "f47e0ae1-982a-4a64-aea3-52506492a3d4";
                configure.UserName = "aa";
                configure.Password = "123456";
                configure.EndPoint = "acm.aliyun.com";
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _namingClient = serviceProvider.GetService<INacosNamingClient>();
            _configClient = serviceProvider.GetService<INacosConfigClient>();
        }
    }
}
