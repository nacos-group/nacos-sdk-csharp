namespace Nacos.V2.Remote.GRpc
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Remote.Requests;
    using System;

    public class GrpcClient : RpcClient
    {
        public GrpcClient(string name)
            : base(name)
        {
        }

        public override RemoteConnection ConnectToServer(RemoteServerInfo serverInfo)
        {
            try
            {
                var channel = CreateNewChannel(serverInfo.ServerIp, serverInfo.ServerPort);

                if (channel != null)
                {
                    var streamClient = new Nacos.BiRequestStream.BiRequestStreamClient(channel);
                    var requestClient = new Nacos.Request.RequestClient(channel);

                    GrpcConnection grpcConn = new GrpcConnection(serverInfo);


                    var streamCall = BindRequestStream(streamClient, grpcConn);

                    // stream observer to send response to server
                    grpcConn.SetBiRequestStreamClient(streamCall);
                    grpcConn.SetRequestClient(requestClient);
                    grpcConn.SetChannel(channel);


                    ConnectionSetupRequest conconSetupRequest = new ConnectionSetupRequest();

                    grpcConn.SendRequest(conconSetupRequest, BuildMeta(conconSetupRequest.GetRemoteType()));
                    return grpcConn;
                }

                return null;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[{0}]Fail to connect to server!", this.GetName());
                return null;
            }
        }

        public override RemoteConnectionType GetConnectionType() => RemoteConnectionType.GRPC;

        public override int RpcPortOffset() => 1000;

        private Grpc.Core.ChannelBase CreateNewChannel(string serverIp, int serverPort)
        {
            var channel = new Grpc.Core.Channel(serverIp, serverPort, Grpc.Core.ChannelCredentials.Insecure);

            bool checkSucess = ServerCheck(channel);

            if (checkSucess)
            {
                return channel;
            }
            else
            {
                ShuntDownChannel(channel);
                return null;
            }
        }

        private void ShuntDownChannel(Grpc.Core.ChannelBase managedChannel)
        {
            if (managedChannel != null)
            {
                managedChannel.ShutdownAsync().GetAwaiter().GetResult();
            }
        }

        private bool ServerCheck(Grpc.Core.ChannelBase channel)
        {
            try
            {
                var payload = GrpcUtils.Convert<object>(new { }, new RequestMeta { Type = RemoteRequestType.Req_ServerCheck });

                var client = new Nacos.Request.RequestClient(channel);
                var resp = client.request(payload);

                return resp != null;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[{0}]Fail to server check!", GetName());
                return false;
            }
        }


        private Grpc.Core.AsyncDuplexStreamingCall<Nacos.Payload, Nacos.Payload> BindRequestStream(Nacos.BiRequestStream.BiRequestStreamClient client, GrpcConnection grpcConn)
        {
            var call = client.requestBiStream();

            System.Threading.Tasks.Task.Factory.StartNew(
               async () =>
               {
                   var cts = new System.Threading.CancellationTokenSource();

                   while (await call.ResponseStream.MoveNext(cts.Token))
                   {
                       var current = call.ResponseStream.Current;

                       var parse = GrpcUtils.Parse(current);

                       var request = (CommonRequest)parse.Body;
                       if (request != null)
                       {
                           try
                           {
                               var response = HandleServerRequest(request, parse.Metadata);
                               response.RequestId = request.RequestId;
                               await call.RequestStream.WriteAsync(GrpcUtils.Convert(response));
                           }
                           catch (Exception)
                           {
                               throw;
                           }
                       }
                   }

                   if (IsRunning() && !grpcConn.IsAbandon())
                   {
                       logger?.LogInformation(" Request Stream onCompleted ,switch server ");

                       if (System.Threading.Interlocked.CompareExchange(ref rpcClientStatus, RpcClientStatus.UNHEALTHY, RpcClientStatus.RUNNING) == RpcClientStatus.RUNNING)
                       {
                           SwitchServerAsync();
                       }
                   }
                   else
                   {
                       logger?.LogInformation("client is not running status ,ignore complete  event ");
                   }
               }, System.Threading.Tasks.TaskCreationOptions.LongRunning);

            return call;
        }
    }
}
