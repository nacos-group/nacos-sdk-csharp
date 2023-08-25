namespace Nacos.Remote.Responses
{
    public class ConfigReSyncResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Config_ReSync;
    }
}
