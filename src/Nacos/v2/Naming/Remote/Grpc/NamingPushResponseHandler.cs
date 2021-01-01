namespace Nacos.Naming.Remote.Grpc
{
    using Nacos.Remote;

    public class NamingPushResponseHandler : IServerRequestHandler
    {
        public CommonResponse RequestReply(Payload payload, global::Grpc.Core.IClientStreamWriter<Payload> streamWriter)
        {
            throw new System.NotImplementedException();
        }

        public CommonResponse RequestReply(CommonRequest request, CommonRequestMeta meta)
        {
            throw new System.NotImplementedException();
        }
    }
}
