namespace Nacos.V2.Remote.GRpc
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Common;
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using System;
    using System.Collections.Generic;

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
                var channel = new Grpc.Core.Channel(
                    serverInfo.ServerIp,
                    serverInfo.ServerPort,
                    Grpc.Core.ChannelCredentials.Insecure,
                    /* keep config and naming using diff channel */
                    new List<Grpc.Core.ChannelOption> { new Grpc.Core.ChannelOption(GetName(), 1) });

                // after nacos alpha2 server check response was changed!!
                var response = ServerCheck(channel);
                if (response == null || response is not ServerCheckResponse scResp)
                {
                    ShuntDownChannel(channel);
                    return null;
                }

                var streamClient = new Nacos.BiRequestStream.BiRequestStreamClient(channel);
                var requestClient = new Nacos.Request.RequestClient(channel);

                GrpcConnection grpcConn = new GrpcConnection(serverInfo);
                grpcConn.SetConnectionId(scResp.ConnectionId);

                var streamCall = BindRequestStream(streamClient, grpcConn);

                // stream observer to send response to server
                grpcConn.SetBiRequestStreamClient(streamCall);
                grpcConn.SetRequestClient(requestClient);
                grpcConn.SetChannel(channel);

                // after nacos alpha2 setup request was changed!!
                ConnectionSetupRequest conSetupRequest = new ConnectionSetupRequest
                {
                    ClientVersion = Constants.CLIENT_VERSION,
                    Labels = labels,
                    Abilities = clientAbilities,
                    Tenant = GetTenant()
                };

                grpcConn.SendRequest(conSetupRequest, BuildMeta(conSetupRequest.GetRemoteType()));
                return grpcConn;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[{0}]Fail to connect to server!", this.GetName());
                return null;
            }
        }

        public override RemoteConnectionType GetConnectionType() => RemoteConnectionType.GRPC;

        public override int RpcPortOffset() => 1000;

        private void ShuntDownChannel(Grpc.Core.ChannelBase managedChannel)
        {
            if (managedChannel != null)
            {
                managedChannel.ShutdownAsync().GetAwaiter().GetResult();
            }
        }

        private CommonResponse ServerCheck(Grpc.Core.ChannelBase channel)
        {
            try
            {
                var request = new ServerCheckRequest();
                var payload = GrpcUtils.Convert(request, new CommonRequestMeta { Type = RemoteRequestType.Req_ServerCheck });

                var client = new Nacos.Request.RequestClient(channel);
                var resp = client.request(payload);

                var res = GrpcUtils.Parse(resp);
                return (CommonResponse)res;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[{0}]Fail to server check!", GetName());
                return null;
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

                       var parseBody = GrpcUtils.Parse(current);

                       var request = (CommonRequest)parseBody;
                       if (request != null)
                       {
                           try
                           {
                               var response = HandleServerRequest(request);
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
