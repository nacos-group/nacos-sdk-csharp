namespace Nacos.Remote.Responses
{
    public class ConfigRemoveResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Config_Remove;
    }
}
