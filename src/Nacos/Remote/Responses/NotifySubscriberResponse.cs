namespace Nacos.Remote.Responses
{
    public class NotifySubscriberResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_Naming_NotifySubscriber;
    }
}
