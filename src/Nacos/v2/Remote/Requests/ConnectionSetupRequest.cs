namespace Nacos.Remote.Requests
{
    public class ConnectionSetupRequest : CommonRequest
    {
        public override string GetGrpcType() => GRpc.GrpcRequestType.ConnectionSetup;
    }
}
