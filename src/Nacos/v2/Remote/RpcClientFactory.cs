namespace Nacos.Remote
{
    using Microsoft.Extensions.Logging;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;

    public class RpcClientFactory
    {
        public static Dictionary<string, RpcClient> ClientMap = new Dictionary<string, RpcClient>();

        public static RpcClient CreateClient(string clientName, RemoteConnectionType connectionType, Dictionary<string, string> labels)
        {
            string clientNameInner = clientName;

            lock (ClientMap)
            {
                if (!ClientMap.TryGetValue(clientNameInner, out var client))
                {
                    RpcClient moduleClient = null;

                    if (connectionType.Type.Equals(RemoteConnectionType.GRPC))
                    {
                        moduleClient = new Nacos.Remote.GRpc.GrpcClient(clientNameInner);
                    }

                    if (moduleClient == null)
                    {
                        throw new ArgumentException("unsupported connection type :" + connectionType.Type);
                    }

                    moduleClient.InitLabels(labels);

                    ClientMap[clientNameInner] = moduleClient;
                    return moduleClient;
                }

                return client;
            }
        }
    }
}
