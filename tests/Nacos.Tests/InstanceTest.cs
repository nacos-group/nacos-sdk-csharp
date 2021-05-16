/*namespace Nacos.Tests
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class InstanceTest : TestBase
    {
        [Fact]
        public async Task RegisterInstance_Should_Succeed()
        {
            var request = new RegisterInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.74",
                Ephemeral = true,
                Port = 9999
            };

            var res = await _namingClient.RegisterInstanceAsync(request).ConfigureAwait(false);
            Assert.True(res);
            await Task.Delay(1000).ConfigureAwait(false);

            // await _namingClient.(rRequest);
            await Task.Delay(10000).ConfigureAwait(false);
        }

        [Fact]
        public async Task SubscribeUnsubscribeAsync_Should_Succeed()
        {
            Action<IEvent> action = x =>
            {
                var @event = x as NamingEvent;
            };

            var requestA = new RegisterInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.74",
                Ephemeral = true,
                Port = 9999
            };

            await _namingClient.SubscribeAsync("testservice", "", action).ConfigureAwait(false);
            await Task.Delay(10000).ConfigureAwait(false);

            var resA = await _namingClient.RegisterInstanceAsync(requestA).ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);

            var requestB = new RegisterInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.75",
                Ephemeral = true,
                Port = 9999
            };

            var resB = await _namingClient.RegisterInstanceAsync(requestB).ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);

            await _namingClient.UnSubscribeAsync("testservice", "", action).ConfigureAwait(false);
            await Task.Delay(10000).ConfigureAwait(false);

            var requestC = new RegisterInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.76",
                Ephemeral = true,
                Port = 9999
            };

            var resC = await _namingClient.RegisterInstanceAsync(requestC).ConfigureAwait(false);
            await Task.Delay(5000).ConfigureAwait(false);
            Assert.True(true);
        }

        [Fact]
        public async Task AddRemoveBeatAsync_Should_Succeed()
        {
            var request = new RegisterInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.74",
                Ephemeral = true,
                Port = 9999
            };

            var resA = await _namingClient.RegisterInstanceAsync(request).ConfigureAwait(false);
            await Task.Delay(10000).ConfigureAwait(false);

            var requestA = new RemoveInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.74",
                Ephemeral = true,
                Port = 9999
            };
            var resB = await _namingClient.RemoveInstanceAsync(requestA).ConfigureAwait(false);
            await Task.Delay(10000).ConfigureAwait(false);
            Assert.True(true);
        }

        [Fact]
        public async Task RemoveInstance_Should_Succeed()
        {
            var request = new RemoveInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.74",
                Ephemeral = true,
                Port = 9999
            };

            var res = await _namingClient.RemoveInstanceAsync(request).ConfigureAwait(false);
            Assert.True(res);
        }

        [Fact]
        public async Task ModifyInstance_Should_Succeed()
        {
            var request = new ModifyInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.74",
                Port = 5000
            };

            var res = await _namingClient.ModifyInstanceAsync(request).ConfigureAwait(false);
            Assert.True(res);
        }

        [Fact]
        public async Task ListInstances_Should_Succeed()
        {
            var request = new ListInstancesRequest
            {
                ServiceName = "testservice",
            };

            var res = await _namingClient.ListInstancesAsync(request).ConfigureAwait(false);
            Assert.NotNull(res);
        }

        [Fact]
        public async Task GetInstance_Should_Succeed()
        {
            var request = new GetInstanceRequest
            {
                ServiceName = "testservice",
                Ip = "192.168.0.74",
                Port = 9999,
            };

            var res = await _namingClient.GetInstanceAsync(request).ConfigureAwait(false);
            Assert.NotNull(res);
        }

        [Fact]
        public async Task SendHeartbeat_Should_Succeed()
        {
            var request = new SendHeartbeatRequest
            {
                ServiceName = "testservice",
                BeatInfo = new BeatInfo
                {
                    serviceName = "testservice",
                    ip = "192.168.0.74",
                    port = 9999,
                }
            };

            var res = await _namingClient.SendHeartbeatAsync(request).ConfigureAwait(false);
            Assert.True(res);
        }

        [Fact]
        public async Task ModifyInstanceHealthStatus_Should_Succeed()
        {
            var request = new ModifyInstanceHealthStatusRequest
            {
                Ip = "192.168.0.74",
                Port = 9999,
                ServiceName = "testservice",
                Healthy = false,
            };

            var res = await _namingClient.ModifyInstanceHealthStatusAsync(request).ConfigureAwait(false);

            // 集群配置了健康检查时,该接口会返回错误
            Assert.False(res);
        }
    }
}
*/