namespace Nacos.Tests.Naming.Remote
{
    using Microsoft.Extensions.Logging.Abstractions;
    using Nacos.V2;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Remote.Grpc;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using Xunit;

    [Trait("Category", "2x")]
    public class NamingPushRequestHandlerTests
    {
        [Fact]
        public void RequestReply_Should_Succeed()
        {
            ServiceInfo info = new ServiceInfo("name", "cluster1");

            var holder = new ServiceInfoHolder(NullLogger.Instance, string.Empty, new NacosSdkOptions(), null);
            var handler = new NamingPushRequestHandler(holder);

            var req = new NotifySubscriberRequest() { ServiceInfo = info };

            var resp = handler.RequestReply(req);

            Assert.IsType<NotifySubscriberResponse>(resp);
        }
    }
}
