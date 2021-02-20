namespace Nacos.V2.Remote.Responses
{
    public class ConfigRemoveResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Config_Remove;
    }
}
