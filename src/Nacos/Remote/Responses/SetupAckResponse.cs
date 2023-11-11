namespace Nacos.Remote.Responses
{
    public class SetupAckResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_SetupAck;
    }
}
