namespace Nacos.V2.Remote
{
    using System;
    using System.Collections.Generic;

    public static class RemoteRequestType
    {
        public static readonly string Req_Config_Publish = "com.alibaba.nacos.api.config.remote.request.ConfigPublishRequest";

        public static readonly string Req_Config_Remove = "com.alibaba.nacos.api.config.remote.request.ConfigRemoveRequest";

        public static readonly string Req_Config_Get = "com.alibaba.nacos.api.config.remote.request.ConfigQueryRequest";

        public static readonly string Req_Config_Listen = "com.alibaba.nacos.api.config.remote.request.ConfigBatchListenRequest";

        public static readonly string Naming_HeartBeat = "com.alibaba.nacos.api.remote.request.HeartBeatRequest";

        public static readonly string Req_Naming_Instance = "com.alibaba.nacos.api.naming.remote.request.InstanceRequest";

        public static readonly string Req_Naming_ServiceQuery = "com.alibaba.nacos.api.naming.remote.request.ServiceQueryRequest";

        public static readonly string Req_Naming_ServiceList = "com.alibaba.nacos.api.naming.remote.request.ServiceListRequest";

        public static readonly string Req_Naming_NotifySubscriber = "com.alibaba.nacos.api.naming.remote.request.NotifySubscriberRequest";

        public static readonly string Req_Naming_SubscribeService = "com.alibaba.nacos.api.naming.remote.request.SubscribeServiceRequest";

        public static readonly string Req_ConnectionSetup = "com.alibaba.nacos.api.remote.request.ConnectionSetupRequest";

        public static readonly string Req_ServerCheck = "com.alibaba.nacos.api.remote.request.ServerCheckRequest";

        public static readonly string Resp_Config_Pubish = "com.alibaba.nacos.api.config.remote.response.ConfigPubishResponse";

        public static readonly string Resp_Config_BatchListen = "com.alibaba.nacos.api.config.remote.response.ConfigChangeBatchListenResponse";

        public static readonly string Resp_Config_ChangeNotify = "com.alibaba.nacos.api.config.remote.response.ConfigChangeNotifyResponse";

        public static readonly string Req_Config_ChangeNotify = "com.alibaba.nacos.api.config.remote.request.ConfigChangeNotifyRequest";

        public static readonly string Resp_Config_Query = "com.alibaba.nacos.api.config.remote.response.ConfigQueryResponse";

        public static readonly string Resp_Config_Remove = "com.alibaba.nacos.api.config.remote.response.ConfigRemoveResponse";

        public static readonly string Resp_Naming_SubscribeServic = "com.alibaba.nacos.api.naming.remote.response.SubscribeServiceResponse";

        public static readonly string Resp_Naming_QueryService = "com.alibaba.nacos.api.naming.remote.response.QueryServiceResponse";

        public static readonly string Resp_Naming_ServiceList = "com.alibaba.nacos.api.naming.remote.response.ServiceListResponse";

        public static readonly string Resp_Naming_Instance = "com.alibaba.nacos.api.naming.remote.response.InstanceResponse";

        public static Dictionary<string, Type> RemoteResponseTypeMapping = new Dictionary<string, Type>
        {
            { Resp_Config_Pubish, typeof(Nacos.V2.Remote.Responses.ConfigPubishResponse) },
            { Resp_Config_BatchListen, typeof(Nacos.V2.Remote.Responses.ConfigChangeBatchListenResponse) },
            { Resp_Config_ChangeNotify, typeof(Nacos.V2.Remote.Responses.ConfigChangeNotifyResponse) },
            { Resp_Config_Query, typeof(Nacos.V2.Remote.Responses.ConfigQueryResponse) },
            { Resp_Config_Remove, typeof(Nacos.V2.Remote.Responses.ConfigRemoveResponse) },
            { Req_Config_ChangeNotify, typeof(Nacos.V2.Remote.Requests.ConfigChangeNotifyRequest) },
            { Req_Naming_NotifySubscriber, typeof(Nacos.V2.Remote.Requests.NotifySubscriberRequest) },
            { Resp_Naming_SubscribeServic, typeof(Nacos.V2.Remote.Responses.SubscribeServiceResponse) },
            { Resp_Naming_QueryService, typeof(Nacos.V2.Remote.Responses.QueryServiceResponse) },
            { Resp_Naming_ServiceList, typeof(Nacos.V2.Remote.Responses.ServiceListResponse) },
            { Resp_Naming_Instance, typeof(Nacos.V2.Remote.Responses.InstanceResponse) },
        };
    }
}
