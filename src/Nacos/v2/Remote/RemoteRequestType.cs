namespace Nacos.Remote
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

        public static readonly string Naming_Instance = "com.alibaba.nacos.api.naming.remote.request.InstanceRequest";

        public static readonly string Naming_ServiceList = "com.alibaba.nacos.api.naming.remote.request.ServiceListRequest";

        public static readonly string Req_ConnectionSetup = "com.alibaba.nacos.api.remote.request.ConnectionSetupRequest";

        public static readonly string Req_ServerCheck = "com.alibaba.nacos.api.remote.request.ServerCheckRequest";

        public static readonly string Resp_Config_Pubish = "com.alibaba.nacos.api.config.remote.response.ConfigPubishResponse";

        public static readonly string Resp_Config_BatchListen = "com.alibaba.nacos.api.config.remote.response.ConfigChangeBatchListenResponse";

        public static readonly string Resp_Config_ChangeNotify = "com.alibaba.nacos.api.config.remote.response.ConfigChangeNotifyResponse";

        public static readonly string Req_Config_ChangeNotify = "com.alibaba.nacos.api.config.remote.request.ConfigChangeNotifyRequest";

        public static readonly string Resp_Config_Query = "com.alibaba.nacos.api.config.remote.response.ConfigQueryResponse";

        public static readonly string Resp_Config_Remove = "com.alibaba.nacos.api.config.remote.response.ConfigRemoveResponse";

        public static Dictionary<string, Type> RemoteResponseTypeMapping = new Dictionary<string, Type>
        {
            { Resp_Config_Pubish, typeof(Nacos.Remote.Responses.ConfigPubishResponse) },
            { Resp_Config_BatchListen, typeof(Nacos.Remote.Responses.ConfigChangeBatchListenResponse) },
            { Resp_Config_ChangeNotify, typeof(Nacos.Remote.Responses.ConfigChangeNotifyResponse) },
            { Resp_Config_Query, typeof(Nacos.Remote.Responses.ConfigQueryResponse) },
            { Resp_Config_Remove, typeof(Nacos.Remote.Responses.ConfigRemoveResponse) },
            { Req_Config_ChangeNotify, typeof(Nacos.Remote.Requests.ConfigChangeNotifyRequest) },
        };
    }
}
