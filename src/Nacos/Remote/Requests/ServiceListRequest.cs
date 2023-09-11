namespace Nacos.Remote.Requests
{
    public class ServiceListRequest : AbstractNamingRequest
    {
        public ServiceListRequest(string @namespace, string serviceName, string groupName)
            : base(@namespace, serviceName, groupName)
        {
        }

        public ServiceListRequest(string @namespace, string groupName, int pageNo, int pageSize)
            : base(@namespace, string.Empty, groupName)
        {
            PageNo = pageNo;
            PageSize = pageSize;
        }

        [System.Text.Json.Serialization.JsonPropertyName("pageNo")]
        public int PageNo { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("selector")]
        public string Selector { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_ServiceList;
    }
}
