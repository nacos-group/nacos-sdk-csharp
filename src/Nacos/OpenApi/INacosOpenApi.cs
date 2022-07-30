namespace Nacos.OpenApi
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface INacosOpenApi
    {
        /// <summary>
        /// Get namespace list.
        /// </summary>
        /// <returns>namespace list</returns>
        Task<List<NacosNamespace>> GetNamespacesAsync();

        /// <summary>
        /// Create namespace
        /// </summary>
        /// <param name="customNamespaceId">ID of namespace</param>
        /// <param name="namespaceName">Name of namespace</param>
        /// <param name="namespaceDesc">Description of namespace</param>
        /// <returns>Created or not</returns>
        Task<bool> CreateNamespaceAsync(string customNamespaceId, string namespaceName, string namespaceDesc);

        /// <summary>
        /// Update namespace
        /// </summary>
        /// <param name="namespaceId">ID of namespace</param>
        /// <param name="namespaceName">Name of namespace</param>
        /// <param name="namespaceDesc">Description of namespace</param>
        /// <returns>Updated or not</returns>
        Task<bool> UpdateNamespaceAsync(string namespaceId, string namespaceName, string namespaceDesc);

        /// <summary>
        /// Delete namespace
        /// </summary>
        /// <param name="namespaceId">ID of namespace</param>
        /// <returns>Deleted or not</returns>
        Task<bool> DeleteNamespaceAsync(string namespaceId);

        /// <summary>
        /// Query system metrics
        /// </summary>
        /// <param name="onlyStatus">only status info or not</param>
        /// <returns>system metrics</returns>
        Task<NacosMetrics> GetMetricsAsync(bool onlyStatus);
    }
}
