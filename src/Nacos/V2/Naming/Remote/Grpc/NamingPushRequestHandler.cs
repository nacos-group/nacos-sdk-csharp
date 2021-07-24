namespace Nacos.V2.Naming.Remote.Grpc
{
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Remote;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;

    public class NamingPushRequestHandler : IServerRequestHandler
    {
        private ServiceInfoHolder _serviceInfoHolder;

        public NamingPushRequestHandler(ServiceInfoHolder serviceInfoHolder)
        {
            this._serviceInfoHolder = serviceInfoHolder;
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
