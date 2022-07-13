namespace Nacos.V2.Naming.Remote.Grpc
{
    using Nacos.V2.Naming.Dtos;
    using System.Collections.Generic;

    public class BatchInstanceRedoData : InstanceRedoData
    {
        public List<Instance> Instances { get; set; }

        public BatchInstanceRedoData(string serviceName, string groupName)
            : base(serviceName, groupName)
        {
        }

        public static BatchInstanceRedoData Build(string serviceName, string groupName, List<Instance> instance)
        {
            var result = new BatchInstanceRedoData(serviceName, groupName)
            {
                Instances = instance
            };
            return result;
        }
    }
}
