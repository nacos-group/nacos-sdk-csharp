﻿[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Nacos.Tests")]

namespace Nacos.Remote
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using Nacos.Remote.Requests;
    using System.Threading;
    using Nacos.Utils;
    using Nacos.Common;
    using Nacos.Exceptions;

    public abstract class RpcClient : IDisposable
    {
        private const string DEFAULT_NACOS_PORT_ENV_NAME = "nacos.server.port";
        private const string DEFAULT_NACOS_PORT_ENV_VALUE = "8848";

        private string _name;
        private string _tenant;
        private long _lastActiveTimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        private int _keepAliveTime = 5000;

        private IServerListFactory _serverListFactory;

        private readonly BlockingCollection<ReconnectContext> _reconnectionSignal = new(boundedCapacity: 1);

        private readonly BlockingCollection<ConnectionEvent> _eventLinkedBlockingQueue = new();

        protected ILogger logger;

        protected TLSConfig _tlsConfig;

        protected Dictionary<string, string> labels = new();

        protected int rpcClientStatus = RpcClientStatus.WAIT_INIT;

        protected RemoteConnection currentConnection;

        protected ClientAbilities clientAbilities;

        /// <summary>
        /// listener called where connect status changed.
        /// </summary>
        protected List<IConnectionEventListener> connectionEventListeners = new();

        /// <summary>
        /// change listeners handler registry.
        /// </summary>
        protected List<IServerRequestHandler> serverRequestHandlers = new();

        public RpcClient(string name, TLSConfig tlsConfig)
        {
            _name = name;
            _tlsConfig = tlsConfig;
        }

        public RpcClient(IServerListFactory serverListFactory)
        {
            _serverListFactory = serverListFactory;

            Interlocked.CompareExchange(ref rpcClientStatus, RpcClientStatus.INITIALIZED, RpcClientStatus.WAIT_INIT);

            logger?.LogInformation("RpcClient init in constructor , ServerListFactory ={0}", _serverListFactory?.GetType()?.Name);
        }

        public RpcClient(string name, TLSConfig tlsConfig, IServerListFactory serverListFactory)
            : this(name, tlsConfig)
        {
            _serverListFactory = serverListFactory;
            Interlocked.CompareExchange(ref rpcClientStatus, RpcClientStatus.INITIALIZED, RpcClientStatus.WAIT_INIT);

            logger?.LogInformation("RpcClient init in constructor , ServerListFactory ={0}", _serverListFactory?.GetType()?.Name);
        }

        protected CommonRequestMeta BuildMeta(string type)
        {
            var meta = new CommonRequestMeta
            {
                ClientVersion = Constants.CLIENT_VERSION,
                Labels = labels,
                ClientIp = NetUtils.LocalIP(),
                Type = type
            };
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
        /// check if current connected server is in serverlist ,if not switch server.
        /// </summary>
        protected void OnServerListChange()
        {
            if (currentConnection != null && currentConnection.ServerInfo != null)
            {
                var serverInfo = currentConnection.ServerInfo;
                bool found = false;
                foreach (var serverAddress in _serverListFactory.GetServerList())
                {
                    if (ResolveServerInfo(serverAddress).GetAddress().Equals(serverInfo.GetAddress(), StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    logger?.LogInformation(
                        "Current connected server {0}  is not in latest server list,switch switchServerAsync",
                        serverInfo.GetAddress());

                    SwitchServerAsync();
                }
            }
        }

        /// <summary>
        /// check is this client is inited.
        /// </summary>
        public bool IsWaitInited() => rpcClientStatus == RpcClientStatus.WAIT_INIT;

        /// <summary>
        /// check is this client is running.
        /// </summary>
        public bool IsRunning() => rpcClientStatus == RpcClientStatus.RUNNING;

        /// <summary>
        /// check is this client is shutdwon.
        /// </summary>
        public bool IsShutdwon() => rpcClientStatus == RpcClientStatus.SHUTDOWN;

        public void Init(IServerListFactory serverListFactory)
        {
            if (!IsWaitInited()) return;

            _serverListFactory = serverListFactory;
            Interlocked.CompareExchange(ref rpcClientStatus, RpcClientStatus.INITIALIZED, RpcClientStatus.WAIT_INIT);

            logger?.LogInformation("RpcClient init in constructor , ServerListFactory ={}", _serverListFactory?.GetType()?.Name);
        }

        public void InitLabels(Dictionary<string, string> labels)
        {
            foreach (var item in labels)
            {
                this.labels[item.Key] = item.Value;
            }

            logger?.LogInformation("RpcClient init label  ,labels={0}", this.labels.ToJsonString());
        }

        public void SetClientAbilities(ClientAbilities clientAbilities) => this.clientAbilities = clientAbilities;

        public string GetTenant() => _tenant;

        public void SetTenant(string tenant) => _tenant = tenant;

        public void Start()
        {
            if (Interlocked.CompareExchange(ref rpcClientStatus, RpcClientStatus.STARTING, RpcClientStatus.INITIALIZED) != RpcClientStatus.INITIALIZED) return;

            StartConnectEvent();

            StartReconnect();

            RemoteConnection connectToServer = null;

            Interlocked.Exchange(ref rpcClientStatus, RpcClientStatus.STARTING);

            int startUpretyTimes = 3;

            while (startUpretyTimes > 0 && connectToServer == null)
            {
                try
                {
                    startUpretyTimes--;
                    RemoteServerInfo serverInfo = NextRpcServer();

                    logger?.LogInformation("[{0}] Try to  connect to server on start up,server : {1}", _name, serverInfo?.ToString());

                    connectToServer = ConnectToServer(serverInfo);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning("[{0}]fail to connect to server on start up,error message={1},start up trytimes left :{2}", _name, ex.Message, startUpretyTimes);
                }
            }

            if (connectToServer != null)
            {
                logger?.LogInformation("[{0}] success to connect to server [{1}] on start up, connectionId={2}", _name, connectToServer.ServerInfo?.GetAddress(), connectToServer.GetConnectionId());
                currentConnection = connectToServer;
                Interlocked.Exchange(ref rpcClientStatus, RpcClientStatus.RUNNING);
                _eventLinkedBlockingQueue.TryAdd(new ConnectionEvent(ConnectionEvent.CONNECTED));
            }
            else
            {
                SwitchServerAsync();
            }

            RegisterServerPushResponseHandler(new ConnectResetRequestHandler(this));
            RegisterServerPushResponseHandler(new ClientDetectionRequestHandler());
        }

        private void StartConnectEvent()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        // block 5000ms
                        if (!_eventLinkedBlockingQueue.TryTake(out var take, 5000)) continue;

                        if (take.IsConnected()) NotifyConnected();
                        else if (take.IsDisConnected()) NotifyDisConnected();
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }
                }
            });
        }

        private void StartReconnect()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        // block 5000ms
                        if (!_reconnectionSignal.TryTake(out var reconnectContext, _keepAliveTime))
                        {
                            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastActiveTimeStamp < _keepAliveTime) continue;

                            bool isHealthy = await DoHealthCheckAsync().ConfigureAwait(false);

                            if (!isHealthy)
                            {
                                if (currentConnection == null) continue;

                                logger?.LogInformation("[{0}]Server healthy check fail,currentConnection={1}", _name, currentConnection.GetConnectionId());

                                if (Interlocked.CompareExchange(ref rpcClientStatus, RpcClientStatus.UNHEALTHY, RpcClientStatus.RUNNING) == RpcClientStatus.RUNNING)
                                {
                                    reconnectContext = new ReconnectContext(null, false);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                _lastActiveTimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                continue;
                            }
                        }

                        if (reconnectContext.ServerInfo != null)
                        {
                            bool serverExist = false;

                            foreach (var server in GetServerListFactory().GetServerList())
                            {
                                var serInfo = ResolveServerInfo(server);
                                if (serInfo.ServerIp.Equals(reconnectContext.ServerInfo.ServerIp))
                                {
                                    serverExist = true;
                                    reconnectContext.ServerInfo.ServerPort = serInfo.ServerPort;
                                    break;
                                }
                            }

                            if (!serverExist)
                            {
                                logger?.LogInformation("[{0}] Recommend server is not in server list ,ignore recommend server {1}", _name, reconnectContext.ServerInfo.GetAddress());
                                reconnectContext.ServerInfo = null;
                            }
                        }

                        await Reconnect(reconnectContext.ServerInfo, reconnectContext.OnRequestFail).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "[ rpc listen execute ] [rpc listen] exception");
                    }
                }
            });
        }

        private async Task<bool> DoHealthCheckAsync()
        {
            var healthCheckRequest = new HealthCheckRequest();
            if (currentConnection == null) return false;

            try
            {
                var response = await currentConnection.RequestAsync(healthCheckRequest, BuildMeta(healthCheckRequest.GetRemoteType()), 3000L).ConfigureAwait(false);

                // not only check server is ok ,also check connection is register.
                return response != null && response.IsSuccess();
            }
            catch
            {
                // ignore
            }

            return false;
        }

        protected async Task Reconnect(RemoteServerInfo recommendServerInfo, bool onRequestFail)
        {
            try
            {
                var recommendServer = recommendServerInfo;
                if (onRequestFail && await DoHealthCheckAsync().ConfigureAwait(false))
                {
                    logger?.LogInformation("[{0}] Server check success : {1}", _name, recommendServer);
                    Interlocked.Exchange(ref rpcClientStatus, RpcClientStatus.RUNNING);
                    return;
                }

                // loop until start client success.
                bool switchSuccess = false;

                int reConnectTimes = 0;
                int retryTurns = 0;
                Exception lastException = null;
                while (!switchSuccess && !IsShutdwon())
                {
                    // 1.get a new server
                    RemoteServerInfo serverInfo = null;
                    try
                    {
                        serverInfo = recommendServer == null ? NextRpcServer() : recommendServer;

                        // 2.create a new channel to new server
                        var connectionNew = ConnectToServer(serverInfo);
                        if (connectionNew != null)
                        {
                            logger?.LogInformation("[{0}] success to connect server : {1}", _name, recommendServer);

                            // successfully create a new connect.
                            if (currentConnection != null)
                            {
                                // set current connection to enable connection event.
                                currentConnection.SetAbandon(true);
                                CloseConnection(currentConnection);
                            }

                            currentConnection = connectionNew;
                            Interlocked.Exchange(ref rpcClientStatus, RpcClientStatus.RUNNING);
                            switchSuccess = true;

                            _eventLinkedBlockingQueue.TryAdd(new ConnectionEvent(ConnectionEvent.CONNECTED));
                            return;
                        }

                        // close connection if client is already shutdown.
                        if (IsShutdwon())
                        {
                            CloseConnection(currentConnection);
                        }

                        lastException = null;
                    }
                    catch (Exception e)
                    {
                        lastException = e;
                    }
                    finally
                    {
                        recommendServer = null;
                    }

                    if (reConnectTimes > 0 && reConnectTimes % _serverListFactory.GetServerList().Count == 0)
                    {
                        logger?.LogInformation("[{0}] fail to connect server,after trying {1} times, last try server is {2}", _name, reConnectTimes, serverInfo);

                        if (retryTurns == int.MaxValue)
                        {
                            retryTurns = 50;
                        }
                        else
                        {
                            retryTurns++;
                        }
                    }

                    reConnectTimes++;

                    try
                    {
                        // sleep x milliseconds to switch next server.
                        if (!IsRunning())
                        {
                            // first round ,try servers at a delay 100ms;second round ,200ms; max delays 5s. to be reconsidered.
                            Thread.Sleep((int)(Math.Min(retryTurns + 1, 50) * 100L));
                        }
                    }
                    catch
                    {
                        // Do  nothing.
                    }
                }

                if (IsShutdwon())
                {
                    logger?.LogInformation("[{0}] client is shutdown ,stop reconnect to server", _name);
                }
            }
            catch (Exception e)
            {
                logger?.LogError(e, "[{0}] fail to  connect to server", _name);
            }
        }

        public abstract RemoteConnection ConnectToServer(RemoteServerInfo serverInfo);

        public abstract RemoteConnectionType GetConnectionType();

        public abstract int RpcPortOffset();

        public RemoteServerInfo GetCurrentServer() => currentConnection != null ? currentConnection.ServerInfo : null;

        public Task<CommonResponse> Request(CommonRequest request) => Request(request, 3000L);

        public async Task<CommonResponse> Request(CommonRequest request, long timeoutMills)
        {
            int retryTimes = 0;
            CommonResponse response = null;
            Exception exceptionToThrow = null;
            long start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            while (retryTimes < 3 && DateTimeOffset.Now.ToUnixTimeMilliseconds() < timeoutMills + start)
            {
                bool waitReconnect = false;

                try
                {
                    if (currentConnection == null)
                    {
                        waitReconnect = true;
                        throw new NacosException(NacosException.CLIENT_DISCONNECT, $"Client not connected,current status: {RpcClientStatus.GetStatusName(rpcClientStatus)}");
                    }

                    response = await currentConnection.RequestAsync(request, BuildMeta(request.GetRemoteType()), timeoutMills).ConfigureAwait(false);

                    if (response == null) throw new NacosException(NacosException.SERVER_ERROR, "Unknown Exception.");


                    /* 项目“Nacos (netstandard2.0)”的未合并的更改
                    在此之前:
                                        if (response is Responses.ErrorResponse)
                    在此之后:
                                        if (response is ErrorResponse)
                    */
                    if (response is Responses.ErrorResponse)
                    {
                        // it means that we are new to this nacos server, because we do not setup the connection!!
                        // here should have a try to reconnect or switch server.
                        if (response.ErrorCode == NacosException.UN_REGISTER)
                        {
                            waitReconnect = true;
                            if (Interlocked.CompareExchange(ref rpcClientStatus, RpcClientStatus.UNHEALTHY, RpcClientStatus.RUNNING) == RpcClientStatus.UNHEALTHY)
                            {
                                SwitchServerAsync();
                            }
                        }

                        throw new NacosException(response.ErrorCode, response.Message);
                    }

                    _lastActiveTimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    return response;
                }
                catch (Exception ex)
                {
                    if (waitReconnect) await Task.Delay((int)Math.Min(100, timeoutMills / 3)).ConfigureAwait(false);

                    logger?.LogError("Fail to send request,request={0},retryTimes={1},errorMesssage={2}", request, retryTimes, ex.Message);

                    exceptionToThrow = ex;
                }

                retryTimes++;
            }

            if (Interlocked.CompareExchange(ref rpcClientStatus, RpcClientStatus.UNHEALTHY, RpcClientStatus.RUNNING) == RpcClientStatus.RUNNING)
                SwitchServerAsyncOnRequestFail();

            if (exceptionToThrow != null)
                throw exceptionToThrow is NacosException e ? e : new NacosException(NacosException.SERVER_ERROR, exceptionToThrow.Message);
            else
                throw new NacosException(NacosException.SERVER_ERROR, "Request fail,Unknown Error");
        }

        private void SwitchServerAsyncOnRequestFail() => SwitchServerAsync(null, true);

        public void SwitchServerAsync() => SwitchServerAsync(null, false);

        public void SwitchServerAsync(RemoteServerInfo serverInfo, bool onRequestFail)
            => _reconnectionSignal.TryAdd(new ReconnectContext(serverInfo, onRequestFail));

        protected CommonResponse HandleServerRequest(CommonRequest request)
        {
            foreach (var serverRequestHandler in serverRequestHandlers)
            {
                var response = serverRequestHandler.RequestReply(request);
                if (response != null) return response;
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
            connectionEventListeners.Add(connectionEventListener);
        }

        /// <summary>
        /// register change listeners ,will be called when server send change notify response th current client.
        /// </summary>
        /// <param name="serverRequestHandler">serverRequestHandler</param>
        public void RegisterServerPushResponseHandler(IServerRequestHandler serverRequestHandler)
        {
            logger?.LogInformation("Registry server push request  handler :{0}", serverRequestHandler.GetType().Name);
            serverRequestHandlers.Add(serverRequestHandler);
        }

        public string GetName() => _name;

        public void SetName(string name) => _name = name;

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

        internal RemoteServerInfo ResolveServerInfo(string serverAddress)
        {
            RemoteServerInfo serverInfo = new RemoteServerInfo();
            if (serverAddress.Contains(Constants.HTTP_PREFIX))
            {
                var arr = serverAddress.TrimEnd('/').Split(':');
                serverInfo.ServerIp = arr[1].Replace("//", "");

                serverInfo.ServerPort = arr.Length >= 3
                    ? Convert.ToInt32(arr[2])
                    : Convert.ToInt32(EnvUtil.GetEnvValue(DEFAULT_NACOS_PORT_ENV_NAME, DEFAULT_NACOS_PORT_ENV_VALUE));
            }
            else
            {
                var arr = serverAddress.TrimEnd('/').Split(':');
                serverInfo.ServerIp = arr[0];
                serverInfo.ServerPort = arr.Length >= 2
                    ? Convert.ToInt32(arr[1])
                    : Convert.ToInt32(EnvUtil.GetEnvValue(DEFAULT_NACOS_PORT_ENV_NAME, DEFAULT_NACOS_PORT_ENV_VALUE));
            }

            return serverInfo;
        }

        public Dictionary<string, string> GetLabels() => labels;

        public void Dispose()
        {
            Interlocked.Exchange(ref rpcClientStatus, RpcClientStatus.SHUTDOWN);
            CloseConnection(currentConnection);
        }

        private void CloseConnection(RemoteConnection connection)
        {
            if (connection != null)
            {
                _eventLinkedBlockingQueue.Add(new ConnectionEvent(ConnectionEvent.DISCONNECTED));
                connection.CloseAsync().GetAwaiter().GetResult();
            }
        }
    }
}
