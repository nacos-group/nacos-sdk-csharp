namespace Nacos.Remote.Requests
{
    public class HealthCheckRequest : CommonRequest
    {
        public override string GetRemoteType() => RemoteRequestType.Req_HealthCheck;
    }
}
