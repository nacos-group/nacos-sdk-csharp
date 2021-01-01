namespace Nacos.Remote.Responses
{
    public class ConfigPubishResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Config_Pubish;
    }
}
