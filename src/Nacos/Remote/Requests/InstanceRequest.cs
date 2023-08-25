namespace Nacos.Remote.Requests
{
    public class InstanceRequest : AbstractNamingRequest
    {
        [Newtonsoft.Json.JsonProperty("type")]
        public string Type { get; set; }

        [Newtonsoft.Json.JsonProperty("instance")]

        /* 项目“Nacos (netstandard2.0)”的未合并的更改
        在此之前:
                public Nacos.Naming.Dtos.Instance Instance { get; set; }
        在此之后:
                public Instance Instance { get; set; }
        */
        public Naming.Dtos.Instance Instance { get; set; }


        /* 项目“Nacos (netstandard2.0)”的未合并的更改
        在此之前:
                public InstanceRequest(string @namespace, string serviceName, string groupName, string type, Nacos.Naming.Dtos.Instance instance)
        在此之后:
                public InstanceRequest(string @namespace, string serviceName, string groupName, string type, Instance instance)
        */
        public InstanceRequest(string @namespace, string serviceName, string groupName, string type, Naming.Dtos.Instance instance)
            : base(@namespace, serviceName, groupName)
        {
            Type = type;
            Instance = instance;
        }

        public InstanceRequest(string @namespace, string serviceName, string groupName)
            : base(@namespace, serviceName, groupName)
        {
        }

        public override string GetRemoteType() => RemoteRequestType.Req_Naming_Instance;
    }
}
