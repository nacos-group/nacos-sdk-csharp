namespace Nacos.Remote.Responses
{
    public class ErrorResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Error;
    }
}
