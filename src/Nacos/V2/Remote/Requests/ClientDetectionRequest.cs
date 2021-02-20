namespace Nacos.V2.Remote.Requests
{
    public class ClientDetectionRequest : CommonRequest
    {
        public override string GetRemoteType() => RemoteRequestType.Req_ClientDetection;
    }
}
