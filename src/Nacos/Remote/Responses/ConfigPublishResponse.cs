namespace Nacos.Remote.Responses
{
    public class ConfigPublishResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Config_Pubish_Alpha2;
    }
}
