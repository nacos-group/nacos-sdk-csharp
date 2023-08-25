namespace Nacos.Remote.Responses
{
    public class HealthCheckResponse : CommonResponse
    {
        public override string GetRemoteType() => RemoteRequestType.Resp_HealthCheck;
    }
}
