namespace Nacos.Remote.Requests
{
    public class ConnectionSetupRequest : CommonRequest
    {
        public override string GetRemoteType() => RemoteRequestType.Req_ConnectionSetup;
    }
}
