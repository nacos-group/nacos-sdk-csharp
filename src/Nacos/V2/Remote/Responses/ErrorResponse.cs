namespace Nacos.V2.Remote.Responses
{
    public class ErrorResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Error;
    }
}
