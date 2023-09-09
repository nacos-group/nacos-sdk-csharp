﻿namespace Nacos.Remote.GRpc
{
    using Nacos.Exceptions;
    using Nacos.Remote;
    using System;
    using System.Threading.Tasks;

    public class GrpcConnection : RemoteConnection
    {
        protected Grpc.Net.Client.GrpcChannel channel;

        protected Nacos.Request.RequestClient reqClient;

        protected Grpc.Core.AsyncDuplexStreamingCall<Nacos.Payload, Nacos.Payload> streamCall;

        public void SetChannel(Grpc.Net.Client.GrpcChannel channel) => this.channel = channel;

        public void SetRequestClient(Nacos.Request.RequestClient client) => reqClient = client;

        public void SetBiRequestStreamClient(Grpc.Core.AsyncDuplexStreamingCall<Nacos.Payload, Nacos.Payload> streamCall) => this.streamCall = streamCall;

        public GrpcConnection(RemoteServerInfo serverInfo) => ServerInfo = serverInfo;

        protected override async Task Close()
        {
            if (reqClient != null) reqClient = null;

            if (channel != null) await channel.ShutdownAsync().ConfigureAwait(false);
        }

        protected override Task<CommonResponse> Request(CommonRequest req, CommonRequestMeta meta) => Request(req, meta, 3000L);

        protected override async Task<CommonResponse> Request(CommonRequest req, CommonRequestMeta meta, long timeoutMills)
        {
            // convert normal request to grpc request
            Payload grpcRequest = GrpcUtils.Convert(req, meta);

            Payload grpcResponse = null;

            try
            {
                var callOptions = default(Grpc.Core.CallOptions).WithDeadline(DateTime.UtcNow.AddMilliseconds(timeoutMills));

                grpcResponse = await reqClient.requestAsync(grpcRequest, callOptions).ResponseAsync
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new NacosException(NacosException.SERVER_ERROR, ex.Message);
            }

            var response = (CommonResponse)GrpcUtils.Parse(grpcResponse);
            return response;
        }

        public void SendRequest(CommonRequest request, CommonRequestMeta meta)
        {
            Payload convert = GrpcUtils.Convert(request, meta);
            streamCall.RequestStream.WriteAsync(convert);
        }
    }
}
