namespace Nacos.Remote
{
    using Microsoft.Extensions.Logging;
    using Nacos.Exceptions;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class RpcClient : IDisposable
    {
        private string _name;

        private IServerListFactory _serverListFactory;

        protected ILogger logger;

        protected Dictionary<string, string> labels = new Dictionary<string, string>();

        protected RpcClientStatus rpcClientStatus = new RpcClientStatus(RpcClientStatus.WAIT_INIT);

        protected RemoteConnection currentConnetion;

        /// <summary>
        /// listener called where connect status changed.
        /// </summary>
        protected List<IConnectionEventListener> connectionEventListeners = new List<IConnectionEventListener>();

        /// <summary>
        /// change listeners handler registry.
        /// </summary>
        protected List<IServerRequestHandler> serverRequestHandlers = new List<IServerRequestHandler>();

        public RpcClient(string name) => this._name = name;

        public RpcClient(IServerListFactory serverListFactory)
        {
            this._serverListFactory = serverListFactory;
            rpcClientStatus.Status = RpcClientStatus.INITED;

            logger?.LogInformation("RpcClient init in constructor , ServerListFactory ={}", this._serverListFactory?.GetType()?.Name);
        }

        public RpcClient(string name, IServerListFactory serverListFactory)
            : this(name)
        {
            this._serverListFactory = serverListFactory;
            rpcClientStatus.Status = RpcClientStatus.INITED;

            logger?.LogInformation("RpcClient init in constructor , ServerListFactory ={}", this._serverListFactory?.GetType()?.Name);
        }

        protected CommonRequestMeta BuildMeta(string type)
        {
            var meta = new CommonRequestMeta { ClientVersion = ConstValue.ClientVersion, Labels = labels, Type = type };
            return meta;
        }

        /// <summary>
        /// Notify when client re connected.
        /// </summary>
        protected void NotifyDisConnected()
        {
            if (connectionEventListeners != null && connectionEventListeners.Any())
            {
                logger?.LogInformation("Notify connection event listeners.");

                foreach (var connectionEventListener in connectionEventListeners)
                {
                    connectionEventListener.OnDisConnected();
                }
            }
        }

        /// <summary>
        /// Notify when client new connected.
        /// </summary>
        protected void NotifyConnected()
        {
            if (connectionEventListeners != null && connectionEventListeners.Any())
            {
                foreach (var connectionEventListener in connectionEventListeners)
                {
                    connectionEventListener.OnConnected();
                }
            }
        }

        /// <summary>
        /// check is this client is inited.
        /// </summary>
        public bool IsWaitInited() => this.rpcClientStatus.Status == RpcClientStatus.WAIT_INIT;

        /// <summary>
        /// check is this client is running.
        /// </summary>
        public bool IsRunning() => this.rpcClientStatus.Status == RpcClientStatus.RUNNING;

        /// <summary>
        /// check is this client is shutdwon.
        /// </summary>
        public bool IsShutdwon() => this.rpcClientStatus.Status == RpcClientStatus.SHUTDOWN;

        public void Init(IServerListFactory serverListFactory)
        {
            if (!IsWaitInited()) return;

            this._serverListFactory = serverListFactory;
            rpcClientStatus.Status = RpcClientStatus.INITED;

            logger?.LogInformation("RpcClient init in constructor , ServerListFactory ={}", this._serverListFactory?.GetType()?.Name);
        }

        public void InitLabels(Dictionary<string, string> labels)
        {
            foreach (var item in labels)
            {
                this.labels[item.Key] = item.Value;
            }

            logger?.LogInformation("RpcClient init label  ,labels={0}", this.labels.ToJsonString());
        }

        public void Start()
        {
            if (this.rpcClientStatus.Status >= RpcClientStatus.STARTING) return;

            RemoteConnection connectToServer = null;
            this.rpcClientStatus.Status = RpcClientStatus.STARTING;

            int startUpretyTimes = 3;

            while (startUpretyTimes > 0 && connectToServer == null)
            {
                try
                {
                    startUpretyTimes--;

                    RemoteServerInfo serverInfo = NextRpcServer();

                    logger?.LogInformation("{0} try to  connect to server on start up,server : {1}", this._name, serverInfo?.ToString());

                    connectToServer = ConnectToServer(serverInfo);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning("fail to connect to server on start up,error message={0},start up trytimes left :{1}", ex.Message, startUpretyTimes);
                }
            }

            if (connectToServer != null)
            {
                logger?.LogInformation("{0} success to connect to server on start up", this._name);
                this.currentConnetion = connectToServer;
                rpcClientStatus.Status = RpcClientStatus.RUNNING;
                /*eventLinkedBlockingQueue.offer(new ConnectionEvent(ConnectionEvent.CONNECTED));*/
            }
            else
            {
                /*switchServerAsync();*/
            }
        }

        public abstract RemoteConnection ConnectToServer(RemoteServerInfo serverInfo);

        public abstract RemoteConnectionType GetConnectionType();

        public abstract int RpcPortOffset();

        public RemoteServerInfo GetCurrentServer()
        {
            if (this.currentConnetion != null)
            {
                return currentConnetion.ServerInfo;
            }

            return null;
        }

        public Task<CommonResponse> Request(CommonRequest request) => Request(request, 3000L);

        public async Task<CommonResponse> Request(CommonRequest request, long timeoutMills)
        {
            int retryTimes = 3;
            CommonResponse response = null;
            Exception exceptionToThrow = null;

            while (retryTimes > 0)
            {
                try
                {
                    if (this.currentConnetion != null && !IsRunning()) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "client not connected.");

                    response = await currentConnetion.RequestAsync(request, BuildMeta(request.GetRemoteType()), timeoutMills);

                    if (response != null)
                    {
                        // TODO UNHEALTHY adn switchServerAsync
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogError("Fail to send request,request={0},errorMesssage={1}", request, ex.Message);
                }

                retryTimes--;
            }

            // TODO UNHEALTHY adn switchServerAsync
            if (exceptionToThrow != null)
            {
                throw new NacosException(NacosException.SERVER_ERROR, exceptionToThrow.Message);
            }

            return null;
        }

        protected CommonResponse HandleServerRequest(CommonRequest request, CommonRequestMeta meta)
        {
            foreach (var serverRequestHandler in serverRequestHandlers)
            {
                var response = serverRequestHandler.RequestReply(request, meta);
                if (response != null)
                {
                    return response;
                }
            }

            return null;
        }

        /// <summary>
        /// register connection handler.will be notified when inner connect changed.
        /// </summary>
        /// <param name="connectionEventListener">connectionEventListener</param>
        public void RegisterConnectionListener(IConnectionEventListener connectionEventListener)
        {
            logger?.LogInformation("Registry connection listener to current client:{0}", connectionEventListener.GetType().Name);
            this.connectionEventListeners.Add(connectionEventListener);
        }

        /// <summary>
        /// register change listeners ,will be called when server send change notify response th current client.
        /// </summary>
        /// <param name="serverRequestHandler">serverRequestHandler</param>
        public void RegisterServerPushResponseHandler(IServerRequestHandler serverRequestHandler)
        {
            logger?.LogInformation("Registry server push request  handler :{0}", serverRequestHandler.GetType().Name);
            this.serverRequestHandlers.Add(serverRequestHandler);
        }

        public string GetName() => this._name;

        public void SetName(string name) => this._name = name;

        public IServerListFactory GetServerListFactory() => _serverListFactory;


        protected RemoteServerInfo NextRpcServer()
        {
            string serverAddress = GetServerListFactory().GenNextServer();
            return ResolveServerInfo(serverAddress);
        }

        protected RemoteServerInfo CurrentRpcServer()
        {
            string serverAddress = GetServerListFactory().GetCurrentServer();
            return ResolveServerInfo(serverAddress);
        }

        private RemoteServerInfo ResolveServerInfo(string serverAddress)
        {
            RemoteServerInfo serverInfo = new RemoteServerInfo();
            serverInfo.ServerPort = RpcPortOffset();
            if (serverAddress.Contains(ConstValue.HTTP_PREFIX))
            {
                var arr = serverAddress.Split(':');
                serverInfo.ServerIp = arr[1].Replace("//", "");
                serverInfo.ServerPort += Convert.ToInt32(arr[2].Replace("//", ""));
            }
            else
            {
                var arr = serverAddress.Split(':');
                serverInfo.ServerIp = arr[0];
                serverInfo.ServerPort += Convert.ToInt32(arr[1]);
            }

            return serverInfo;
        }

        public Dictionary<string, string> GetLabels() => this.labels;

        public void Dispose()
        {
            // executorService.shutdown();
            rpcClientStatus.Status = RpcClientStatus.SHUTDOWN;
            CloseConnection(currentConnetion);
        }

        private void CloseConnection(RemoteConnection connection)
        {
            if (connection != null)
            {
                // eventLinkedBlockingQueue.add(new ConnectionEvent(ConnectionEvent.DISCONNECTED));
                connection.CloseAsync().GetAwaiter().GetResult();
            }
        }
    }
}
