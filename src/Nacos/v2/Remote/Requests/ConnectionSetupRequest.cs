namespace Nacos.V2.Remote.Requests
{
    public class ConnectionSetupRequest : CommonRequest
    {
        public override string GetRemoteType() => RemoteRequestType.Req_ConnectionSetup;
    }
}
