﻿namespace Nacos.V2.Remote.Requests
{
    public class InstanceRequest : AbstractNamingRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("instance")]
        public Nacos.V2.Naming.Dtos.Instance Instance { get; set; }

        public InstanceRequest(string @namespace, string serviceName, string groupName, string type, Nacos.V2.Naming.Dtos.Instance instance)
            : base(@namespace, serviceName, groupName)
        {
            this.Type = type;
            this.Instance = instance;
        }

        public InstanceRequest(string @namespace, string serviceName, string groupName)
            : base(@namespace, serviceName, groupName)
        {
        }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_Instance;
    }
}
