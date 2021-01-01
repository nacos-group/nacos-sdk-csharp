namespace Nacos.Naming.Remote.Http
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Nacos.Naming.Dtos;

    public class NamingHttpClientProxy : INamingClientProxy
    {
        public Task CreateService(Service service, AbstractSelector selector)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> DeleteService(string serviceName, string groupName)
        {
            throw new System.NotImplementedException();
        }

        public Task DeregisterService(string serviceName, string groupName, Instance instance)
        {
            throw new System.NotImplementedException();
        }

        public List<string> GetServiceList(int pageNo, int pageSize, string groupName, AbstractSelector selector)
        {
            throw new System.NotImplementedException();
        }

        public Task<Dtos.ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, int udpPort, bool healthyOnly)
        {
            throw new System.NotImplementedException();
        }

        public Service QueryService(string serviceName, string groupName)
        {
            throw new System.NotImplementedException();
        }

        public Task RegisterServiceAsync(string serviceName, string groupName, Instance instance)
        {
            throw new System.NotImplementedException();
        }

        public bool ServerHealthy()
        {
            throw new System.NotImplementedException();
        }

        public Dtos.ServiceInfo Subscribe(string serviceName, string groupName, string clusters)
        {
            throw new System.NotImplementedException();
        }

        public Task Unsubscribe(string serviceName, string groupName, string clusters)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateBeatInfo(List<Instance> modifiedInstances)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateInstance(string serviceName, string groupName, Instance instance)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateService(Service service, AbstractSelector selector)
        {
            throw new System.NotImplementedException();
        }
    }
}
