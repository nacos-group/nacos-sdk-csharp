namespace Nacos.V2
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Remote;

    public interface INacosNamingService
    {
        /// <summary>
        /// register a instance to service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="ip"> instance ip</param>
        /// <param name="port">instance port</param>
        Task RegisterInstance(string serviceName, string ip, int port);

        /// <summary>
        /// register a instance to service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="ip"> instance ip</param>
        /// <param name="port">instance port</param>
        Task RegisterInstance(string serviceName, string groupName, string ip, int port);

        /// <summary>
        /// register a instance to service with specified cluster name.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="ip"> instance ip</param>
        /// <param name="port">instance port</param>
        /// <param name="clusterName">instance cluster name</param>
        Task RegisterInstance(string serviceName, string ip, int port, string clusterName);

        /// <summary>
        /// register a instance to service with specified cluster name.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="ip"> instance ip</param>
        /// <param name="port">instance port</param>
        /// <param name="clusterName">instance cluster name</param>
        Task RegisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

        /// <summary>
        /// register a instance to service with specified instance properties.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="instance">instance to register</param>
        Task RegisterInstance(string serviceName, Instance instance);

        /// <summary>
        /// register a instance to service with specified instance properties.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="instance">instance to register</param>
        Task RegisterInstance(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// deregister instance from a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="ip">instance ip</param>
        /// <param name="port"> instance port</param>
        Task DeregisterInstance(string serviceName, string ip, int port);

        /// <summary>
        /// deregister instance from a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="ip"> instance ip</param>
        /// <param name="port">instance port</param>
        Task DeregisterInstance(string serviceName, string groupName, string ip, int port);

        /// <summary>
        /// deregister instance with specified cluster name from a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="ip"> instance ip</param>
        /// <param name="port">instance port</param>
        /// <param name="clusterName">instance cluster name</param>
        Task DeregisterInstance(string serviceName, string ip, int port, string clusterName);

        /// <summary>
        /// deregister instance with specified cluster name from a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="ip"> instance ip</param>
        /// <param name="port">instance port</param>
        /// <param name="clusterName">instance cluster name</param>
        Task DeregisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

        /// <summary>
        /// deregister instance with full instance information and default groupName.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="instance">instance to register</param>
        Task DeregisterInstance(string serviceName, Instance instance);

        /// <summary>
        /// deregister instance with full instance information.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="instance">instance to register</param>
        Task DeregisterInstance(string serviceName, string groupName, Instance instance);

        /// <summary>
        /// get all instances of a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <returns> A list of instance</returns>
        Task<List<Instance>> GetAllInstances(string serviceName);

        /// <summary>
        /// get all instances of a service.
        /// </summary>
        /// <param name="serviceName"> name of service</param>
        /// <param name="groupName">group of service</param>
        /// <returns> A list of instance</returns>
        Task<List<Instance>> GetAllInstances(string serviceName, string groupName);

        /// <summary>
        /// Get all instances of a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>A list of instance</returns>
        Task<List<Instance>> GetAllInstances(string serviceName, bool subscribe);

        /// <summary>
        /// Get all instances of a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>A list of instance</returns>
        Task<List<Instance>> GetAllInstances(string serviceName, string groupName, bool subscribe);

        /// <summary>
        /// Get all instances within specified clusters of a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <returns>A list of qualified instance</returns>
        Task<List<Instance>> GetAllInstances(string serviceName, List<string> clusters);

        /// <summary>
        ///  Get all instances within specified clusters of a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <returns>A list of qualified instance</returns>
        Task<List<Instance>> GetAllInstances(string serviceName, string groupName, List<string> clusters);

        /// <summary>
        /// Get all instances within specified clusters of a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>A list of qualified instance</returns>
        Task<List<Instance>> GetAllInstances(string serviceName, List<string> clusters, bool subscribe);

        /// <summary>
        /// Get all instances within specified clusters of a service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>A list of qualified instance</returns>
        Task<List<Instance>> GetAllInstances(string serviceName, string groupName, List<string> clusters, bool subscribe);

        /// <summary>
        /// Get qualified instances of service.
        /// </summary>
        /// <param name="serviceName">name of service.</param>
        /// <param name="healthy">a flag to indicate returning healthy or unhealthy instances</param>
        /// <returns> A qualified list of instance</returns>
        Task<List<Instance>> SelectInstances(string serviceName, bool healthy);

        /// <summary>
        /// Get qualified instances of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="healthy">a flag to indicate returning healthy or unhealthy instances</param>
        /// <returns>A qualified list of instance</returns>
        Task<List<Instance>> SelectInstances(string serviceName, string groupName, bool healthy);

        /// <summary>
        /// Get qualified instances of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="healthy">a flag to indicate returning healthy or unhealthy instances</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>A qualified list of instance</returns>
        Task<List<Instance>> SelectInstances(string serviceName, bool healthy, bool subscribe);

        /// <summary>
        /// Get qualified instances of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="healthy">a flag to indicate returning healthy or unhealthy instances</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>A qualified list of instance</returns>
        Task<List<Instance>> SelectInstances(string serviceName, string groupName, bool healthy, bool subscribe);

        /// <summary>
        /// Get qualified instances within specified clusters of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="healthy">a flag to indicate returning healthy or unhealthy instances</param>
        /// <returns>A qualified list of instance</returns>
        Task<List<Instance>> SelectInstances(string serviceName, List<string> clusters, bool healthy);

        /// <summary>
        /// Get qualified instances within specified clusters of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="healthy">a flag to indicate returning healthy or unhealthy instances</param>
        /// <returns>A qualified list of instance</returns>
        Task<List<Instance>> SelectInstances(string serviceName, string groupName, List<string> clusters, bool healthy);

        /// <summary>
        /// Get qualified instances within specified clusters of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="healthy">a flag to indicate returning healthy or unhealthy instances</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>A qualified list of instance</returns>
        Task<List<Instance>> SelectInstances(string serviceName, List<string> clusters, bool healthy, bool subscribe);

        /// <summary>
        /// Get qualified instances within specified clusters of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="healthy">a flag to indicate returning healthy or unhealthy instances</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>A qualified list of instance</returns>
        Task<List<Instance>> SelectInstances(string serviceName, string groupName, List<string> clusters, bool healthy, bool subscribe);

        /// <summary>
        /// Select one healthy instance of service using predefined load balance strategy.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <returns>qualified instance</returns>
        Task<Instance> SelectOneHealthyInstance(string serviceName);

        /// <summary>
        /// Select one healthy instance of service using predefined load balance strategy.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <returns>qualified instance</returns>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName);

        /// <summary>
        /// Select one healthy instance of service using predefined load balance strategy.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>qualified instance</returns>
        Task<Instance> SelectOneHealthyInstance(string serviceName, bool subscribe);

        /// <summary>
        /// Select one healthy instance of service using predefined load balance strategy.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>qualified instance</returns>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, bool subscribe);

        /// <summary>
        /// Select one healthy instance of service using predefined load balance strategy.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <returns>qualified instance</returns>
        Task<Instance> SelectOneHealthyInstance(string serviceName, List<string> clusters);

        /// <summary>
        /// Select one healthy instance of service using predefined load balance strategy.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <returns>qualified instance</returns>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, List<string> clusters);

        /// <summary>
        /// Select one healthy instance of service using predefined load balance strategy.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>qualified instance</returns>
        Task<Instance> SelectOneHealthyInstance(string serviceName, List<string> clusters, bool subscribe);

        /// <summary>
        /// Select one healthy instance of service using predefined load balance strategy.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="subscribe">if subscribe the service</param>
        /// <returns>qualified instance</returns>
        Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, List<string> clusters, bool subscribe);

        /// <summary>
        /// Subscribe service to receive events of instances alteration.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="listener">event listener</param>
        Task Subscribe(string serviceName, IEventListener listener);

        /// <summary>
        /// Subscribe service to receive events of instances alteration.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="listener">event listener</param>
        Task Subscribe(string serviceName, string groupName, IEventListener listener);

        /// <summary>
        /// Subscribe service to receive events of instances alteration.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="listener">event listener</param>
        Task Subscribe(string serviceName, List<string> clusters, IEventListener listener);

        /// <summary>
        /// Subscribe service to receive events of instances alteration.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="listener">event listener</param>
        Task Subscribe(string serviceName, string groupName, List<string> clusters, IEventListener listener);

        /// <summary>
        /// Unsubscribe event listener of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="listener">event listener</param>
        Task Unsubscribe(string serviceName, IEventListener listener);

        /// <summary>
        /// Unsubscribe event listener of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="listener">event listener</param>
        Task Unsubscribe(string serviceName, string groupName, IEventListener listener);

        /// <summary>
        /// Unsubscribe event listener of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="listener">event listener</param>
        Task Unsubscribe(string serviceName, List<string> clusters, IEventListener listener);

        /// <summary>
        /// Unsubscribe event listener of service.
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">group of service</param>
        /// <param name="clusters">list of cluster</param>
        /// <param name="listener">event listener</param>
        Task Unsubscribe(string serviceName, string groupName, List<string> clusters, IEventListener listener);

        /// <summary>
        /// Get all service names from server.
        /// </summary>
        /// <param name="pageNo">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns>list of service names</returns>
        Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize);

        /// <summary>
        /// Get all service names from server.
        /// </summary>
        /// <param name="pageNo">page index</param>
        /// <param name="pageSize">page size</param>
        /// <param name="groupName">group name</param>
        /// <returns>list of service names</returns>
        Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, string groupName);

        /// <summary>
        /// Get all service names from server.
        /// </summary>
        /// <param name="pageNo">page index</param>
        /// <param name="pageSize">page size</param>
        /// <param name="selector">selector to filter the resource</param>
        /// <returns>list of service names</returns>
        Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, AbstractSelector selector);

        /// <summary>
        /// Get all service names from server.
        /// </summary>
        /// <param name="pageNo">page index</param>
        /// <param name="pageSize">page size</param>
        /// <param name="groupName">group name</param>
        /// <param name="selector">selector to filter the resource</param>
        /// <returns>list of service names</returns>
        Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, string groupName, AbstractSelector selector);

        /// <summary>
        /// Get all subscribed services of current client.
        /// </summary>
        /// <returns>subscribed services</returns>
        Task<List<ServiceInfo>> GetSubscribeServices();

        /// <summary>
        /// get server health status.
        /// </summary>
        /// <returns>is server healthy</returns>
        Task<string> GetServerStatus();

        /// <summary>
        /// Shutdown the resource service.
        /// </summary>
        Task ShutDown();
    }
}