namespace Nacos.Remote.Responses
{
    public class QueryServiceResponse : CommonResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("serviceInfo")]

        /* 项目“Nacos (netstandard2.0)”的未合并的更改
        在此之前:
                public Nacos.Naming.Dtos.ServiceInfo ServiceInfo { get; set; }
        在此之后:
                public ServiceInfo ServiceInfo { get; set; }
        */
        public Naming.Dtos.ServiceInfo ServiceInfo { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Naming_QueryService;
    }
}
