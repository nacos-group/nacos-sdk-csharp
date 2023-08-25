namespace Nacos.Remote
{
    public interface IServerRequestHandler
    {
        CommonResponse RequestReply(CommonRequest request);
    }
}
