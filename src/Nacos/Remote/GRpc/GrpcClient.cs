namespace Nacos.Remote.GRpc
{
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using Nacos.Common;
    using Nacos.Remote;
    using Nacos.Remote.Requests;
    using Nacos.Remote.Responses;
    using System;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;

    public class GrpcClient : RpcClient
    {
        private static readonly string NACOS_SERVER_GRPC_PORT_OFFSET_KEY = "nacos.server.grpc.port.offset";
        private static readonly string NACOS_SERVER_GRPC_PORT_DEFAULT_OFFSET = "1000";

        public GrpcClient(string name, TLSConfig tlsConfig)
            : base(name, tlsConfig)
        {
        }

        public override RemoteConnection ConnectToServer(RemoteServerInfo serverInfo)
        {
            try
            {
                var options = new Grpc.Net.Client.GrpcChannelOptions();
                var port = serverInfo.ServerPort + RpcPortOffset();
                var address = string.Empty;

                if (_tlsConfig != null && _tlsConfig.Enabled)
                {
                    var clientCertificate = new X509Certificate2(_tlsConfig.PfxFile, _tlsConfig.Password);

#if !NETSTANDARD2_0
                    var httpClientHandler = new SocketsHttpHandler();
                    httpClientHandler.UseProxy = false;
                    httpClientHandler.AllowAutoRedirect = false;
                    httpClientHandler.SslOptions.ClientCertificates = new X509CertificateCollection { clientCertificate };
                    httpClientHandler.SslOptions.RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true;
#else
                    var httpClientHandler = new HttpClientHandler();
                    httpClientHandler.UseProxy = false;
                    httpClientHandler.AllowAutoRedirect = false;
                    httpClientHandler.ClientCertificates.Add(clientCertificate);
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

                    options.HttpHandler = httpClientHandler;
                    address = $"https://{serverInfo.ServerIp}:{port}";
                }
                else
                {
                    options.Credentials = ChannelCredentials.Insecure;

                    address = $"http://{serverInfo.ServerIp}:{port}";
                }

                options.MaxRetryAttempts = 0;
                var channel = Grpc.Net.Client.GrpcChannel.ForAddress(address, options);

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
                logger?.LogError(ex, "[{0}]Fail to connect to server!", GetName());
                return null;
            }
        }

        public override RemoteConnectionType GetConnectionType() => RemoteConnectionType.GRPC;

        public override int RpcPortOffset() => Convert.ToInt32(Utils.EnvUtil.GetEnvValue(NACOS_SERVER_GRPC_PORT_OFFSET_KEY, NACOS_SERVER_GRPC_PORT_DEFAULT_OFFSET));

        private void ShuntDownChannel(Grpc.Core.ChannelBase managedChannel)
        {
            if (managedChannel != null)
            {
                managedChannel.ShutdownAsync().GetAwaiter().GetResult();
            }
        }

        private CommonResponse ServerCheck(Grpc.Net.Client.GrpcChannel channel)
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

                   while (await call.ResponseStream.MoveNext(cts.Token).ConfigureAwait(false))
                   {
                       var current = call.ResponseStream.Current;

                       var parseBody = GrpcUtils.Parse(current);

                       var request = (CommonRequest)parseBody;
                       if (request != null)
                       {
                           try
                           {
                               if (request is SetupAckRequest)
                               {
                                   // there is no connection ready this time
                                   return;
                               }

                               var response = HandleServerRequest(request);
                               response.RequestId = request.RequestId;
                               await call.RequestStream.WriteAsync(GrpcUtils.Convert(response)).ConfigureAwait(false);
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
