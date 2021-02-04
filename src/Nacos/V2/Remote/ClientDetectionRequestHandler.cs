namespace Nacos.V2.Remote
{
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;

    internal class ClientDetectionRequestHandler : IServerRequestHandler
    {
        public CommonResponse RequestReply(CommonRequest request)
        {
            if (request is ClientDetectionRequest)
            {
                return new ClientDetectionResponse();
            }

            return null;
        }
    }
}