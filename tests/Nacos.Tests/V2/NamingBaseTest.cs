namespace Nacos.Tests.V2
{
    using Nacos.V2;
    using Nacos.V2.Utils;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public abstract class NamingBaseTest
    {
        protected INacosNamingService _namingSvc;

        protected ITestOutputHelper _output;

        [Fact]
        protected virtual async Task RegisterInstance_Should_Succeed()
        {
            var serviceName = $"reg-{Guid.NewGuid().ToString()}";
            var ip = "127.0.0.1";
            var port = 9999;

            await AssertRegisterSingleInstance(serviceName, ip, port, nameof(RegisterInstance_Should_Succeed));
        }

        [Fact]
        protected virtual async Task DeregisterInstance_Should_Succeed()
        {
            var serviceName = $"dereg-{Guid.NewGuid().ToString()}";
            var ip = "127.0.0.2";
            var port = 9999;

            await AssertRegisterSingleInstance(serviceName, ip, port, nameof(DeregisterInstance_Should_Succeed));

            await _namingSvc.DeregisterInstance(serviceName, ip, port);
            await Task.Delay(1000);
            var instances = await _namingSvc.GetAllInstances(serviceName, false);
            _output.WriteLine($"DeregisterInstance_Should_Succeed, GetAllInstances, {serviceName}, {instances?.ToJsonString()}");
            Assert.Empty(instances);
        }

        [Fact]
        protected virtual async Task Subscribe_Should_Succeed()
        {
            var serviceName = $"sub-{Guid.NewGuid().ToString()}";
            var ip = "127.0.0.3";
            var port = 9999;

            await AssertRegisterSingleInstance(serviceName, ip, port, nameof(DeregisterInstance_Should_Succeed));

            var listerner = new NamingListerner(_output);

            await _namingSvc.Subscribe(serviceName, listerner);

            await _namingSvc.RegisterInstance(serviceName, "127.0.0.4", 9999);

            await Task.Delay(500);
        }

        private async Task AssertRegisterSingleInstance(string serviceName, string ip, int port, string testName)
        {
            _output.WriteLine($"{testName}, register instance, {serviceName} ,{ip} , {port}");
            await _namingSvc.RegisterInstance(serviceName, ip, port);
            await Task.Delay(500);
            var instances = await _namingSvc.GetAllInstances(serviceName, false);
            _output.WriteLine($"{testName}, GetAllInstances, {serviceName}, {instances?.ToJsonString()}");
            Assert.Single(instances);

            var first = instances[0];
            Assert.Equal(ip, first.Ip);
            Assert.Equal(port, first.Port);
        }

        public class NamingListerner : Nacos.V2.IEventListener
        {
            private ITestOutputHelper _output;

            public NamingListerner(ITestOutputHelper output) => _output = output;

            public Task OnEvent(IEvent @event)
            {
                _output.WriteLine($"NamingListerner, {@event.ToJsonString()}");

                Assert.IsType<Nacos.V2.Naming.Event.InstancesChangeEvent>(@event);

                return Task.CompletedTask;
            }
        }
    }
}
