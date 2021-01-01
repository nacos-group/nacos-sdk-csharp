namespace Nacos.Remote.GRpc
{
    using Nacos.Remote.Requests;
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

                    grpcConn.SendRequest(conconSetupRequest, BuildMeta(conconSetupRequest.GetGrpcType()));
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public override RemoteConnectionType GetConnectionType() => new RemoteConnectionType(RemoteConnectionType.GRPC);

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
                var payload = GrpcUtils.Convert<object>(new { }, new RequestMeta { Type = GrpcRequestType.ServerCheck });

                var client = new Nacos.Request.RequestClient(channel);
                var resp = client.request(payload);

                return resp != null;
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
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

                        /*var resp = HandleServerRequest(current, call.RequestStream);*/
                   }

                   if (IsRunning() && !grpcConn.IsAbandon())
                   {
                       // LoggerUtils.printIfErrorEnabled(LOGGER, " Request Stream onCompleted ,switch server ");
                       /*if (rpcClientStatus.compareAndSet(RpcClientStatus.RUNNING, RpcClientStatus.UNHEALTHY))
                       {
                           switchServerAsync();
                       }*/
                   }
                   else
                   {
                       /*LoggerUtils.printIfErrorEnabled(LOGGER, "client is not running status ,ignore complete  event ");*/
                   }
               }, System.Threading.Tasks.TaskCreationOptions.LongRunning);

            return call;
        }
    }
}
