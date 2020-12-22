namespace Nacos.Remote.GRpc
{
    using Nacos.Utilities;

    public class GrpcSdkClient
    {
        private string _name;

        public GrpcSdkClient(string name)
        {
            this._name = name;
        }

        public static readonly int RpcPortOffset = 1000;

        public Grpc.Core.ChannelBase ConnectToServer(string address)
        {
            // convert nacos address to grpc address
            // http://ip:port => http://ip:(port + RpcPortOffset)
            var arr = address.TrimEnd('/').Split(':');
            var port = 8848;
            if (arr.Length == 3) port = int.Parse(arr[2]);

            var channel = new Grpc.Core.Channel(arr[1].Replace("//", ""), port + RpcPortOffset, Grpc.Core.ChannelCredentials.Insecure);
            if (ServerCheck(channel))
            {
                BindRequestStream(channel);
            }

            return channel;
        }

        private void BindRequestStream(Grpc.Core.ChannelBase channel)
        {
            var streamClient = new Nacos.BiRequestStream.BiRequestStreamClient(channel);

            var payload = GrpcUtils.Convert<object>(new { }, new RequestMeta { Type = GrpcRequestType.ConnectionSetup });

            var call = streamClient.requestBiStream();

            System.Threading.Tasks.Task.Factory.StartNew(
                async () =>
                {
                    var cts = new System.Threading.CancellationTokenSource();

                    while (await call.ResponseStream.MoveNext(cts.Token))
                    {
                        var current = call.ResponseStream.Current;
#if DEBUG
                        System.Diagnostics.Trace.WriteLine($"ConnectionSetup return {current.Body.Value.ToStringUtf8()}, {current.Metadata.ToJsonString()}");
#endif
                    }
                }, System.Threading.Tasks.TaskCreationOptions.LongRunning);

            call.RequestStream.WriteAsync(payload).GetAwaiter().GetResult();
            call.RequestStream.CompleteAsync().GetAwaiter().GetResult();
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
    }
}
