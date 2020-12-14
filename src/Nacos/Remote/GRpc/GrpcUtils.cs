namespace Nacos.Remote.GRpc
{
    using Nacos.Utilities;
    using System.Linq;

    public static class GrpcUtils
    {
        public static Payload Convert<T>(T request, RequestMeta meta)
        {
            var body = new Google.Protobuf.WellKnownTypes.Any
            {
                // convert the request paramter to a json string, as the body
                Value = Google.Protobuf.ByteString.CopyFromUtf8(request.ToJsonString())
            };

            var payload = new Payload
            {
                Body = body
            };

            if (meta != null)
            {
                payload.Metadata = new Metadata
                {
                    ClientIp = meta.ClientIp,
                    ClientPort = meta.ClientPort,
                    ClientVersion = meta.ClientVersion,
                    ConnectionId = meta.ConnectionId,
                    Type = meta.Type,
                };

                if (meta.Labels != null && meta.Labels.Any())
                    foreach (var item in meta.Labels) payload.Metadata.Labels.Add(item.Key, item.Value);
            }

            return payload;
        }

        public static string Convert(Payload payload)
        {
            var retStr = payload.Body.Value.ToStringUtf8();

            System.Diagnostics.Trace.WriteLine($" convert response result, {retStr} ");

            return retStr;
        }
    }
}
