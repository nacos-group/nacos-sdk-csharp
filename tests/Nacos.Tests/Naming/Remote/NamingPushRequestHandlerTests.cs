namespace Nacos.Tests.Naming.Remote
{
    using Microsoft.Extensions.Logging.Abstractions;
    using Nacos.Naming.Dtos;
    using Nacos.Remote.Responses;
    using Nacos;
    using Nacos.Naming.Cache;
    using Nacos.Naming.Remote.Grpc;
    using Nacos.Remote.Requests;
    using Xunit;

    [Trait("Category", "2x")]
    public class NamingPushRequestHandlerTests
    {
        [Fact]
        public void RequestReply_Should_Succeed()
        {
            ServiceInfo info = new ServiceInfo("name", "cluster1");

            var holder = new ServiceInfoHolder(string.Empty, new NacosSdkOptions(), null);
            var handler = new NamingPushRequestHandler(holder);

            var req = new NotifySubscriberRequest() { ServiceInfo = info };

            var resp = handler.RequestReply(req);

            Assert.IsType<NotifySubscriberResponse>(resp);
        }
    }
}
