namespace Nacos.AspNetCore
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [System.Obsolete("This interface is obsolete and will be removed in a future version. Use INacosNamingService instead.")]
    public interface INacosServerManager
    {
        /// <summary>
        /// Get the server URL with specify load balance strategy.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <returns>A URL string</returns>
        Task<string> GetServerAsync(string serviceName);

        /// <summary>
        /// Get the server URL with specify load balance strategy.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <returns>A URL string</returns>
        Task<string> GetServerAsync(string serviceName, string groupName);

        /// <summary>
        /// Get the server URL with specify load balance strategy.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="clusters">clusters</param>
        /// <returns>A URL string</returns>
        Task<string> GetServerAsync(string serviceName, string groupName, string clusters);

        /// <summary>
        /// Get the server URL with specify load balance strategy.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="clusters">clusters</param>
        /// <param name="namespaceId">namespace id</param>
        /// <returns>A URL string</returns>
        Task<string> GetServerAsync(string serviceName, string groupName, string clusters, string namespaceId);

        /// <summary>
        /// Get the HOST information with specify load balance strategy.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <returns>The HOST information</returns>
        Task<Host> GetServerInfoAsync(string serviceName);

        /// <summary>
        /// Get the HOST information with specify load balance strategy.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <returns>The HOST information</returns>
        Task<Host> GetServerInfoAsync(string serviceName, string groupName);

        /// <summary>
        /// Get the HOST information with specify load balance strategy.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="clusters">clusters</param>
        /// <returns>The HOST information</returns>
        Task<Host> GetServerInfoAsync(string serviceName, string groupName, string clusters);

        /// <summary>
        /// Get the HOST information with specify load balance strategy.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="clusters">clusters</param>
        /// <param name="namespaceId">namespace id</param>
        /// <returns>The HOST information</returns>
        Task<Host> GetServerInfoAsync(string serviceName, string groupName, string clusters, string namespaceId);

        /// <summary>
        /// Get a list of HOST information.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <returns>A list of HOST information</returns>
        Task<List<Host>> GetServerListAsync(string serviceName);

        /// <summary>
        /// Get a list of HOST information.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <returns>A list of HOST information</returns>
        Task<List<Host>> GetServerListAsync(string serviceName, string groupName);

        /// <summary>
        /// Get a list of HOST information.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="clusters">clusters</param>
        /// <returns>A list of HOST information</returns>
        Task<List<Host>> GetServerListAsync(string serviceName, string groupName, string clusters);

        /// <summary>
        /// Get a list of HOST information.
        /// </summary>
        /// <param name="serviceName">service name</param>
        /// <param name="groupName">group name</param>
        /// <param name="clusters">clusters</param>
        /// <param name="namespaceId">namespace id</param>
        /// <returns>A list of HOST information</returns>
        Task<List<Host>> GetServerListAsync(string serviceName, string groupName, string clusters, string namespaceId);
    }
}
