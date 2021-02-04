namespace Nacos.V2.Remote.Responses
{
    public class ClientDetectionResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_ClientDetection;
    }
}
