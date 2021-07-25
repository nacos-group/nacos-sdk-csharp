namespace Nacos.V2.Naming.Remote.Grpc
{
    using Nacos.V2.Naming.Dtos;

    public class InstanceRedoData : RedoData<Instance>
    {
        public InstanceRedoData(string serviceName, string groupName)
            : base(serviceName, groupName)
        {
        }

        /// <summary>
        /// Build a new RedoData for register service instance.
        /// </summary>
        /// <param name="serviceName">service name for redo data</param>
        /// <param name="groupName">group name for redo data</param>
        /// <param name="instance">instance for redo data</param>
        /// <returns>new RedoData for register service instance</returns>
        public static InstanceRedoData Build(string serviceName, string groupName, Instance instance)
        {
            var result = new InstanceRedoData(serviceName, groupName)
            {
                Data = instance
            };
            return result;
        }
    }
}
