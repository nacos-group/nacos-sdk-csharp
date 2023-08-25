namespace Nacos.Naming.Remote.Grpc
{
    using Nacos.Naming.Cache;
    using Nacos.Remote;
    using Nacos.Remote.Requests;
    using Nacos.Remote.Responses;

    public class NamingPushRequestHandler : IServerRequestHandler
    {
        private ServiceInfoHolder _serviceInfoHolder;

        public NamingPushRequestHandler(ServiceInfoHolder serviceInfoHolder)
        {
            _serviceInfoHolder = serviceInfoHolder;
        }

        public CommonResponse RequestReply(CommonRequest request)
        {
            if (request is NotifySubscriberRequest req)
            {
                _serviceInfoHolder.ProcessServiceInfo(req.ServiceInfo);
                return new NotifySubscriberResponse() { RequestId = req.RequestId };
            }

            return null;
        }
    }
}
