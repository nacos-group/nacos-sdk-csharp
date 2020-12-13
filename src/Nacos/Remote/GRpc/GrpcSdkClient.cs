namespace Nacos.Remote.GRpc
{
    using Grpc.Net.Client;
    using System;
    using System.Threading.Tasks;

    public class GrpcSdkClient
    {
        private string _name;

        public GrpcSdkClient(string name)
        {
            this._name = name;
        }

        public int RpcPortOffset()
        {
            return 1000;
        }

        public GrpcChannel ConnectToServer(string address)
        {
            var channel = GrpcChannel.ForAddress(address);
            BindRequestStream(channel);
            return channel;
        }

        private void BindRequestStream(GrpcChannel channel)
        {
            var streamClient = new Nacos.BiRequestStream.BiRequestStreamClient(channel);

            var payload = GrpcUtils.Convert<object>(new { }, null);

            var call = streamClient.requestBiStream();

            var cts = new System.Threading.CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                while (await call.ResponseStream.MoveNext(cts.Token))
                {
                    var resp = call.ResponseStream.Current;

                    Console.WriteLine($" bi 返回body的value = {resp.Body.Value.ToStringUtf8()}");
                    Console.WriteLine($" bi 返回metadata = {Newtonsoft.Json.JsonConvert.SerializeObject(resp.Metadata)}");
                }
            });

            call.RequestStream.WriteAsync(payload).GetAwaiter().GetResult();
            call.RequestStream.CompleteAsync().GetAwaiter().GetResult();
        }
    }
}
