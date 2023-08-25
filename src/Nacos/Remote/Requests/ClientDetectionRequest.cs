namespace Nacos.Remote.Requests
{
    public class ClientDetectionRequest : CommonRequest
    {
        public override string GetRemoteType() => RemoteRequestType.Req_ClientDetection;
    }
}
