namespace Nacos.Remote.Requests
{
    public class ServerCheckRequest : CommonRequest
    {
        public override string GetRemoteType() => RemoteRequestType.Req_ServerCheck;
    }
}
