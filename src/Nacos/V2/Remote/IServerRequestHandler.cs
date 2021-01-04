namespace Nacos.V2.Remote
{
    public interface IServerRequestHandler
    {
        CommonResponse RequestReply(CommonRequest request, CommonRequestMeta meta);
    }
}
