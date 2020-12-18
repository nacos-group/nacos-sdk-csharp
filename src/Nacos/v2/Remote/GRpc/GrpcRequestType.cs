namespace Nacos.Remote.GRpc
{
    public class GrpcRequestType
    {
        public static readonly string Config_Publish = "com.alibaba.nacos.api.config.remote.request.ConfigPublishRequest";

        public static readonly string Config_Remove = "com.alibaba.nacos.api.config.remote.request.ConfigRemoveRequest";

        public static readonly string Config_Get = "com.alibaba.nacos.api.config.remote.request.ConfigQueryRequest";

        public static readonly string Config_Listen = "com.alibaba.nacos.api.config.remote.request.ConfigBatchListenRequest";

        public static readonly string Naming_HeartBeat = "com.alibaba.nacos.api.remote.request.HeartBeatRequest";

        public static readonly string Naming_Instance = "com.alibaba.nacos.api.naming.remote.request.InstanceRequest";

        public static readonly string Naming_ServiceList = "com.alibaba.nacos.api.naming.remote.request.ServiceListRequest";

        public static readonly string ConnectionSetup = "com.alibaba.nacos.api.remote.request.ConnectionSetupRequest";
    }
}
