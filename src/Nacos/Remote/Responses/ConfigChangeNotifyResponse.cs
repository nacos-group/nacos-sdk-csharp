namespace Nacos.Remote.Responses
{
    public class ConfigChangeNotifyResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Config_ChangeNotify;
    }
}
