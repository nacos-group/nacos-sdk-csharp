namespace Nacos.V2.Naming.Remote.Grpc
{
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Remote;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;

    public class NamingPushResponseHandler : IServerRequestHandler
    {
        private ServiceInfoHolder _serviceInfoHolder;

        public NamingPushResponseHandler(ServiceInfoHolder serviceInfoHolder)
        {
            this._serviceInfoHolder = serviceInfoHolder;
        }

        public CommonResponse RequestReply(CommonRequest request, CommonRequestMeta meta)
        {
            if (request is NotifySubscriberRequest)
            {
                var req = (NotifySubscriberRequest)request;
                _serviceInfoHolder.ProcessServiceInfo(req.ServiceInfo);
                return new NotifySubscriberResponse() { RequestId = req.RequestId };
            }

            return null;
        }
    }
}
