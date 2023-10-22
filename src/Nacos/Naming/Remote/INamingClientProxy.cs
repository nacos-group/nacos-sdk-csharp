namespace Nacos.Naming.Remote
{
    using Nacos.Naming.Dtos;
    using Nacos.Remote;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface INamingClientProxy
    {
        /// <summary>
        /// Register a instance to service with specified instance properties.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="instance">instance to register</param>
        Task RegisterServiceAsync(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// batch register instance to service with specified instance properties.
        /// since nacos server 2.1.1
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="instances">instances to register</param>
        Task BatchRegisterServiceAsync(string serviceName, string groupName, List<Instance> instances);

        /// <summary>
        /// Batch deRegister instance to service with specified instance properties.
        /// since nacos server 2.2.0
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="instances">instances to register</param>
        Task BatchDeregisterServiceAsync(string serviceName, string groupName, List<Instance> instances);

        Task DeregisterService(string serviceName, string groupName, Instance instance);

        Task UpdateInstance(string serviceName, string groupName, Instance instance);

        Task<ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, bool healthyOnly);

        Task<Service> QueryService(string serviceName, string groupName);


        Task CreateService(Service service, AbstractSelector selector);


        Task<bool> DeleteService(string serviceName, string groupName);


        Task UpdateService(Service service, AbstractSelector selector);


        Task<ListView<string>> GetServiceList(int pageNo, int pageSize, string groupName, AbstractSelector selector);

        Task<ServiceInfo> Subscribe(string serviceName, string groupName, string clusters);

        Task Unsubscribe(string serviceName, string groupName, string clusters);

        bool ServerHealthy();

        Task<bool> IsSubscribed(string serviceName, string groupName, string clusters);
    }

    public interface INamingHttpClientProxy : INamingClientProxy, IDisposable
    {
    }

    public interface INamingGrpcClientProxy : INamingClientProxy, IDisposable
    {
    }
}
