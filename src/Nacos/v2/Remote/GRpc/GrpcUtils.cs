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

        public static T Parse<T>(Payload payload)
        {
            var retStr = payload.Body.Value.ToStringUtf8();

            System.Diagnostics.Trace.WriteLine($" convert response result, {retStr} ");

            return retStr.ToObj<T>();
        }

        public static Requests.PlainRequest Parse(Payload payload)
        {
            Requests.PlainRequest plainRequest = new Requests.PlainRequest();

            // com.alibaba.nacos.api.config.remote.response.ConfigPubishResponse
            var type = payload.Metadata.Type;

            if (RemoteRequestType.RemoteResponseTypeMapping.TryGetValue(type, out var classType))
            {
                var retStr = payload.Body.Value.ToStringUtf8();
                System.Diagnostics.Trace.WriteLine($"parse response result, {retStr} ");
                object obj = retStr.ToObj(classType);
                plainRequest.Body = obj;
            }

            plainRequest.Type = type;
            plainRequest.Metadata = ConvertMeta(payload.Metadata);
            return plainRequest;
        }

        private static CommonRequestMeta ConvertMeta(Nacos.Metadata metadata)
        {
            var requestMeta = new CommonRequestMeta()
            {
                ClientIp = metadata.ClientIp,
                ClientPort = metadata.ClientPort,
                ConnectionId = metadata.ConnectionId,
                ClientVersion = metadata.ClientVersion,
                Type = metadata.Type
            };

            if (metadata.Labels != null && metadata.Labels.Any())
            {
                foreach (var item in metadata.Labels)
                {
                    requestMeta.Labels[item.Key] = item.Value;
                }
            }


            return requestMeta;
        }

        public static Payload Convert(CommonRequest request, CommonRequestMeta meta)
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

        public static Payload Convert(CommonResponse response)
        {
            var body = new Google.Protobuf.WellKnownTypes.Any
            {
                // convert the request paramter to a json string, as the body
                Value = Google.Protobuf.ByteString.CopyFromUtf8(response.ToJsonString())
            };

            var payload = new Payload
            {
                Body = body
            };

            payload.Metadata = new Metadata
            {
                ClientVersion = ConstValue.ClientVersion,
                Type = response.GetRemoteType(),
            };

            return payload;
        }
    }
}
