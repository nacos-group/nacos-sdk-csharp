namespace Nacos.V2.Remote
{
    using Nacos.V2.Common;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using Nacos.V2.Utils;

    internal class ConnectResetRequestHandler : IServerRequestHandler
    {
        private readonly RpcClient _rpcClient;

        public ConnectResetRequestHandler(RpcClient rpcClient)
        {
            this._rpcClient = rpcClient;
        }

        public CommonResponse RequestReply(CommonRequest request)
        {
            if (request is ConnectResetRequest connectResetRequest)
            {
                try
                {
                    if (_rpcClient.IsRunning())
                    {
                        if (connectResetRequest.ServerIp.IsNotNullOrWhiteSpace())
                        {
                            var serverInfo = _rpcClient.ResolveServerInfo(connectResetRequest.ServerIp + Constants.COLON + connectResetRequest.ServerPort);

                            _rpcClient.SwitchServerAsync(serverInfo, false);
                        }
                        else
                        {
                            _rpcClient.SwitchServerAsync();
                        }
                    }
                }
                catch (System.Exception)
                {
                }

                return new ConnectResetResponse();
            }

            return null;
        }
    }
}