namespace Nacos.V2.Remote.Requests
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
            this.PageNo = pageNo;
            this.PageSize = pageSize;
        }

        [Newtonsoft.Json.JsonProperty("pageNo")]
        public int PageNo { get; set; }

        [Newtonsoft.Json.JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [Newtonsoft.Json.JsonProperty("selector")]
        public string Selector { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_ServiceList;
    }
}
