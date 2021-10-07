namespace Nacos.Tests.Remote
{
    using Nacos.V2.Remote;
    using System;
    using System.Collections.Generic;
    using Xunit;

    [Trait("Category", "2x")]
    public class RpcClientTests
    {
        [Fact]
        public void ResolveServerInfo_With_HTTPPrefix_Should_Succeed()
        {
            var rpcClient = RpcClientFactory.CreateClient("name", RemoteConnectionType.GRPC, new Dictionary<string, string>() { });

            var a1 = rpcClient.ResolveServerInfo("http://localhost:8848");
            Assert.Equal("localhost:8848", a1.GetAddress());

            var a2 = rpcClient.ResolveServerInfo("https://10.0.0.2:8843");
            Assert.Equal("10.0.0.2:8843", a2.GetAddress());

            var a3 = rpcClient.ResolveServerInfo("https://nacos.example.com");
            Assert.Equal("nacos.example.com:8848", a3.GetAddress());

            var a4 = rpcClient.ResolveServerInfo("https://nacos.example.com:8890");
            Assert.Equal("nacos.example.com:8890", a4.GetAddress());
        }

        [Fact]
        public void ResolveServerInfo_Without_HTTPPrefix_Should_Succeed()
        {
            var rpcClient = RpcClientFactory.CreateClient("name", RemoteConnectionType.GRPC, new Dictionary<string, string>() { });

            var a1 = rpcClient.ResolveServerInfo("localhost:8848");
            Assert.Equal("localhost:8848", a1.GetAddress());

            var a2 = rpcClient.ResolveServerInfo("10.0.0.2:8843");
            Assert.Equal("10.0.0.2:8843", a2.GetAddress());

            var a3 = rpcClient.ResolveServerInfo("nacos.example.com");
            Assert.Equal("nacos.example.com:8848", a3.GetAddress());

            var a4 = rpcClient.ResolveServerInfo("nacos.example.com:8890");
            Assert.Equal("nacos.example.com:8890", a4.GetAddress());
        }

        [Fact]
        public void ResolveServerInfo_With_EnvironmentVariable_Should_Succeed()
        {
            var rpcClient = RpcClientFactory.CreateClient("name", RemoteConnectionType.GRPC, new Dictionary<string, string>() { });

            Environment.SetEnvironmentVariable("nacos.server.port", "9090");
            var a1 = rpcClient.ResolveServerInfo("localhost");
            Assert.Equal("localhost:9090", a1.GetAddress());

            var a2 = rpcClient.ResolveServerInfo("10.0.0.2:8843");
            Assert.Equal("10.0.0.2:8843", a2.GetAddress());

            var a3 = rpcClient.ResolveServerInfo("nacos.example.com");
            Assert.Equal("nacos.example.com:9090", a3.GetAddress());
        }
    }
}
