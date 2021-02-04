namespace Nacos.V2.Remote
{
    using Nacos.V2.Remote.Requests;
    using Nacos.V2.Remote.Responses;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// nacos 2.0.0.alpha1 fullname
    /// nacos 2.0.0.alpha2 fullname
    /// after nacos 2.0.0.alpha2 simplename
    /// </summary>
    public static class RemoteRequestType
    {
        public static readonly string Req_Config_Publish = "ConfigPublishRequest";

        public static readonly string Req_Config_Remove = "ConfigRemoveRequest";

        public static readonly string Req_Config_Get = "ConfigQueryRequest";

        public static readonly string Req_Config_Listen = "ConfigBatchListenRequest";

        public static readonly string Naming_HeartBeat = "HeartBeatRequest";

        public static readonly string Req_Naming_Instance = "InstanceRequest";

        public static readonly string Req_Naming_ServiceQuery = "ServiceQueryRequest";

        public static readonly string Req_Naming_ServiceList = "ServiceListRequest";

        public static readonly string Req_Naming_NotifySubscriber = "NotifySubscriberRequest";

        public static readonly string Req_Naming_SubscribeService = "SubscribeServiceRequest";

        public static readonly string Req_ConnectionSetup = "ConnectionSetupRequest";

        public static readonly string Req_ServerCheck = "ServerCheckRequest";

        public static readonly string Req_Config_ReSync = "ConfigReSyncRequest";

        public static readonly string Resp_Config_Pubish_Alpha2 = "ConfigPublishResponse";

        public static readonly string Resp_Config_Pubish_Alpha1 = "ConfigPubishResponse";

        public static readonly string Resp_Config_BatchListen = "ConfigChangeBatchListenResponse";

        public static readonly string Resp_Config_ChangeNotify = "ConfigChangeNotifyResponse";

        public static readonly string Req_Config_ChangeNotify = "ConfigChangeNotifyRequest";

        public static readonly string Resp_Config_Query = "ConfigQueryResponse";

        public static readonly string Resp_Config_Remove = "ConfigRemoveResponse";

        public static readonly string Resp_Naming_SubscribeServic = "SubscribeServiceResponse";

        public static readonly string Resp_Naming_QueryService = "QueryServiceResponse";

        public static readonly string Resp_Naming_ServiceList = "ServiceListResponse";

        public static readonly string Resp_Naming_Instance = "InstanceResponse";

        public static readonly string Resp_ConnectionUnregister = "ConnectionUnregisterResponse";

        public static readonly string Resp_Config_ReSync = "ConfigReSyncResponse";

        public static readonly string Resp_Error = "ErrorResponse";

        public static readonly string Resp_ServerCheck = "ServerCheckResponse";

        public static readonly string Req_PushAck = "PushAckRequest";

        public static readonly string Req_ConnectReset = "ConnectResetRequest";

        public static readonly string Resp_ConnectReset = "ConnectResetResponse";

        public static readonly string Req_HealthCheck = "HealthCheckRequest";

        public static readonly string Resp_HealthCheck = "HealthCheckResponse";

        public static readonly string Req_ClientDetection = "ClientDetectionRequest";

        public static readonly string Resp_ClientDetection = "ClientDetectionResponse";

        public static Dictionary<string, Type> RemoteResponseTypeMapping = new Dictionary<string, Type>
        {
            { Resp_Config_Pubish_Alpha1, typeof(ConfigPubishResponse) },
            { Resp_Config_Pubish_Alpha2, typeof(ConfigPublishResponse) },
            { Resp_Config_BatchListen, typeof(ConfigChangeBatchListenResponse) },
            { Resp_Config_ChangeNotify, typeof(ConfigChangeNotifyResponse) },
            { Resp_Config_Query, typeof(ConfigQueryResponse) },
            { Resp_Config_Remove, typeof(ConfigRemoveResponse) },
            { Req_Config_ChangeNotify, typeof(ConfigChangeNotifyRequest) },
            { Req_Naming_NotifySubscriber, typeof(NotifySubscriberRequest) },
            { Resp_Naming_SubscribeServic, typeof(SubscribeServiceResponse) },
            { Resp_Naming_QueryService, typeof(QueryServiceResponse) },
            { Resp_Naming_ServiceList, typeof(ServiceListResponse) },
            { Resp_Naming_Instance, typeof(InstanceResponse) },
            { Resp_ConnectionUnregister, typeof(ConnectionUnregisterResponse) },
            { Resp_Config_ReSync, typeof(ConfigReSyncResponse) },
            { Req_Config_ReSync, typeof(ConfigReSyncRequest) },
            { Resp_Error, typeof(ErrorResponse) },
            { Resp_ServerCheck, typeof(ServerCheckResponse) },
            { Req_ClientDetection, typeof(ClientDetectionRequest) },
            { Req_ConnectReset, typeof(ConnectResetRequest) },
            { Resp_HealthCheck, typeof(HealthCheckResponse) },
        };
    }
}
