namespace Nacos.V2.Remote
{
    public interface IServerRequestHandler
    {
        CommonResponse RequestReply(Payload payload, Grpc.Core.IClientStreamWriter<Payload> streamWriter);

        CommonResponse RequestReply(CommonRequest request, CommonRequestMeta meta);
    }
}
