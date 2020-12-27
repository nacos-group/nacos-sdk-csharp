namespace Nacos.Remote.GRpc
{
    public interface IServerRequestHandler
    {
        CommonResponse RequestReply(Payload payload, Grpc.Core.IClientStreamWriter<Payload> streamWriter);
    }
}
