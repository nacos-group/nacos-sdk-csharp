﻿namespace Nacos.Remote
{
    using Nacos.Remote.GRpc;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class RpcClientFactory
    {
        public static ConcurrentDictionary<string, RpcClient> ClientMap = new ConcurrentDictionary<string, RpcClient>();

        public static RpcClient CreateClient(string clientName, RemoteConnectionType connectionType, Dictionary<string, string> labels, TLSConfig tlsConfig = null)
        {
            string clientNameInner = clientName;

            if (!ClientMap.TryGetValue(clientNameInner, out var client))
            {
                RpcClient moduleClient = null;

                if (connectionType.Equals(RemoteConnectionType.GRPC))
                {
                    moduleClient = new GrpcClient(clientNameInner, tlsConfig);
                }

                if (moduleClient == null)
                {
                    throw new ArgumentException("unsupported connection type :" + connectionType.ToString());
                }

                moduleClient.InitLabels(labels);

                ClientMap.AddOrUpdate(clientNameInner, moduleClient, (x, y) => moduleClient);

                ClientMap[clientNameInner] = moduleClient;
                return moduleClient;
            }

            return client;
        }
    }
}
