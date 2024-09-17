﻿namespace Nacos.V2.Remote.Responses
{
    using System.Collections.Generic;

    public class ServiceListResponse : CommonResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("count")]
        public int Count { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("serviceNames")]
        public List<string> ServiceNames { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Resp_Naming_ServiceList;
    }
}
