namespace Nacos.Remote.GRpc
{
    using System;
    using System.Threading.Tasks;

    public class GrpcConnection : RemoteConnection
    {
        protected Grpc.Core.ChannelBase channel;

        protected Nacos.Request.RequestClient reqClient;

        protected Grpc.Core.AsyncDuplexStreamingCall<Nacos.Payload, Nacos.Payload> streamCall;

        public void SetChannel(Grpc.Core.ChannelBase channel) => this.channel = channel;

        public void SetRequestClient(Nacos.Request.RequestClient client) => this.reqClient = client;

        public void SetBiRequestStreamClient(Grpc.Core.AsyncDuplexStreamingCall<Nacos.Payload, Nacos.Payload> streamCall) => this.streamCall = streamCall;

        public GrpcConnection(RemoteServerInfo serverInfo) => this.ServerInfo = serverInfo;

        protected override async Task Close()
        {
            if (reqClient != null) reqClient = null;

            if (channel != null) await channel.ShutdownAsync();
        }

        protected override Task<CommonResponse> Request(CommonRequest req, CommonRequestMeta meta) => Request(req, meta, 3000L);

        protected override async Task<CommonResponse> Request(CommonRequest req, CommonRequestMeta meta, long timeoutMills)
        {
            // convert normal request to grpc request
            Payload grpcRequest = GrpcUtils.Convert(req, meta);

            Payload grpcResponse = null;

            try
            {
                var callOptions = default(Grpc.Core.CallOptions).WithDeadline(DateTime.Now.AddMilliseconds(timeoutMills));

                grpcResponse = await reqClient.requestAsync(grpcRequest, callOptions);
            }
            catch (System.Exception ex)
            {
                throw new Nacos.Exceptions.NacosException(Nacos.ConstValue.SERVER_ERROR, ex.Message);
            }

            var response = GrpcUtils.Parse(grpcResponse);
            return response;
        }

        public void SendResponse(CommonResponse response)
        {
            /*Payload convert = GrpcUtils.convert(response);
            payloadStreamObserver.onNext(convert);*/
        }

        public void SendRequest(CommonRequest request, CommonRequestMeta meta)
        {
            Payload convert = GrpcUtils.Convert(request, meta);
            streamCall.RequestStream.WriteAsync(convert);
        }


        internal bool IsAbandon()
        {
            throw new NotImplementedException();
        }
    }
}
