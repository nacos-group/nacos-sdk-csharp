namespace Nacos.Remote
{
    using Nacos.Remote.Requests;
    using Nacos.Remote.Responses;

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