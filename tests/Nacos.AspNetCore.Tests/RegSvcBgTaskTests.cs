namespace Nacos.AspNetCore.Tests
{
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using Nacos.AspNetCore.V2;
    using Nacos.V2;
    using Nacos.V2.Naming.Dtos;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    [Trait("Category", "all")]
    public class RegSvcBgTaskTests
    {
        private Mock<INacosNamingService> _mockSvc;
        private Mock<IServer> _mockServer;
        private Mock<IOptionsMonitor<NacosAspNetOptions>> _mockOptions;

        public RegSvcBgTaskTests()
        {
            _mockSvc = new Mock<INacosNamingService>();
            _mockServer = new Mock<IServer>();
            _mockOptions = new Mock<IOptionsMonitor<NacosAspNetOptions>>();
        }

        [Fact]
        public async Task StartAsync_RegisterInstance_Should_Call_Only_Once()
        {
            _mockServer.Setup(x => x.Features).Returns(new FeatureCollection());
            _mockSvc.Setup(x => x.RegisterInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Instance>())).Returns(Task.CompletedTask);
            _mockOptions.Setup(x => x.CurrentValue).Returns(new NacosAspNetOptions { ServiceName = "abc123" });

            var task = new RegSvcBgTask(NullLoggerFactory.Instance, _mockSvc.Object, _mockServer.Object, _mockOptions.Object);
            await task.StartAsync(default).ConfigureAwait(false);

            // only one, if succeed
            _mockSvc.Verify(x => x.RegisterInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Instance>()), Times.Once);
        }

        [Fact]
        public async Task StartAsync_RegisterInstance_Should_Call_AtMost_Three_Times()
        {
            _mockServer.Setup(x => x.Features).Returns(new FeatureCollection());
            _mockSvc.Setup(x => x.RegisterInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Instance>())).Throws<Exception>();
            _mockOptions.Setup(x => x.CurrentValue).Returns(new NacosAspNetOptions { ServiceName = "abc123" });

            var task = new RegSvcBgTask(NullLoggerFactory.Instance, _mockSvc.Object, _mockServer.Object, _mockOptions.Object);
            await task.StartAsync(default).ConfigureAwait(false);

            // at most three times, if fail
            _mockSvc.Verify(x => x.RegisterInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Instance>()), Times.AtMost(3));
        }

        [Fact]
        public async Task StartAsync_RegisterInstance_Should_Call_Only_Twice_When_SAF_Contains_Two_Addresses()
        {
            var fc = new FeatureCollection();
            IServerAddressesFeature saf = new ServerAddressesFeature();
            saf.Addresses.Add("http://*:8080");
            saf.Addresses.Add("http://*:8081");
            fc.Set(saf);

            _mockServer.Setup(x => x.Features).Returns(fc);
            _mockSvc.Setup(x => x.RegisterInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Instance>())).Returns(Task.CompletedTask);
            _mockOptions.Setup(x => x.CurrentValue).Returns(new NacosAspNetOptions { ServiceName = "abc123" });

            var task = new RegSvcBgTask(NullLoggerFactory.Instance, _mockSvc.Object, _mockServer.Object, _mockOptions.Object);
            await task.StartAsync(default).ConfigureAwait(false);

            _mockSvc.Verify(x => x.RegisterInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Instance>()), Times.Exactly(2));
        }

        [Fact]
        public async Task StartAsync_RegisterInstance_Should_Call_AtMost_Six_Times_When_SAF_Contains_Two_Addresses()
        {
            var fc = new FeatureCollection();
            IServerAddressesFeature saf = new ServerAddressesFeature();
            saf.Addresses.Add("http://*:8080");
            saf.Addresses.Add("http://*:8081");
            fc.Set(saf);

            _mockServer.Setup(x => x.Features).Returns(fc);
            _mockSvc.Setup(x => x.RegisterInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Instance>())).Throws<Exception>();
            _mockOptions.Setup(x => x.CurrentValue).Returns(new NacosAspNetOptions { ServiceName = "abc123" });

            var task = new RegSvcBgTask(NullLoggerFactory.Instance, _mockSvc.Object, _mockServer.Object, _mockOptions.Object);
            await task.StartAsync(default).ConfigureAwait(false);

            _mockSvc.Verify(x => x.RegisterInstance(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Instance>()), Times.AtMost(6));
        }
    }
}
