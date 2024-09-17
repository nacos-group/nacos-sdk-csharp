namespace Nacos.V2.Remote.Responses
{
    public class SetupAckResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_SetupAck;
    }
}
