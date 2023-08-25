namespace Nacos.Remote.Responses
{
    public class ConnectionUnregisterResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_ConnectionUnregister;
    }
}
