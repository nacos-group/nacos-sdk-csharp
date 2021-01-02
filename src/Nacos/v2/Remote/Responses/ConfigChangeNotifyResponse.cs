namespace Nacos.V2.Remote.Responses
{
    public class ConfigChangeNotifyResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Config_ChangeNotify;
    }
}
