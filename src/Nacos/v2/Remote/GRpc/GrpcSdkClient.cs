namespace Nacos.Remote.GRpc
{
    using Grpc.Net.Client;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class GrpcSdkClient
    {
        private string _name;

        public GrpcSdkClient(string name)
        {
            this._name = name;
        }

        public static readonly int RpcPortOffset = 1000;

        public GrpcChannel ConnectToServer(string address)
        {
            // convert nacos address to grpc address
            // http://ip:port => http://ip:(port + RpcPortOffset)
            var arr = address.TrimEnd('/').Split(":");
            var port = 8848;
            if (arr.Length == 3) port = int.Parse(arr[2]);

            var url = $"{arr[0]}:{arr[1]}:{port + RpcPortOffset}";
            var channel = GrpcChannel.ForAddress(url);
            if (ServerCheck(channel))
            {
                BindRequestStream(channel);
            }

            return channel;
        }

        private void BindRequestStream(GrpcChannel channel)
        {
            var streamClient = new Nacos.BiRequestStream.BiRequestStreamClient(channel);

            var payload = GrpcUtils.Convert<object>(new { }, new RequestMeta { Type = GrpcRequestType.ConnectionSetup });

            var call = streamClient.requestBiStream();

            call.RequestStream.WriteAsync(payload).GetAwaiter().GetResult();
            call.RequestStream.CompleteAsync().GetAwaiter().GetResult();
        }

        private bool ServerCheck(GrpcChannel channel)
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
