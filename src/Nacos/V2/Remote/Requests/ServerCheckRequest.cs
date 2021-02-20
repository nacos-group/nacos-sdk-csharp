namespace Nacos.V2.Remote.Requests
{
    public class ServerCheckRequest : CommonRequest
    {
        public override string GetRemoteType() => RemoteRequestType.Req_ServerCheck;
    }
}
