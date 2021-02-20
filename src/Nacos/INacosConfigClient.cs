namespace Nacos
{
    using System.Threading.Tasks;

    public interface INacosConfigClient
    {
        string Name { get; }

        /// <summary>
        /// Gets configurations in Nacos
        /// </summary>
        /// <param name="request">request</param>
        /// <returns>string</returns>
        Task<string> GetConfigAsync(GetConfigRequest request);

        /// <summary>
        /// Publishes configurations in Nacos
        /// </summary>
        /// <param name="request">request</param>
        /// <returns>bool</returns>
        Task<bool> PublishConfigAsync(PublishConfigRequest request);

        /// <summary>
        /// Deletes configurations in Nacos
        /// </summary>
        /// <param name="request">request</param>
        /// <returns>bool</returns>
        Task<bool> RemoveConfigAsync(RemoveConfigRequest request);

        /// <summary>
        /// Listen configuration.
        /// </summary>
        /// <param name="request">request.</param>
        Task AddListenerAsync(AddListenerRequest request);

        /// <summary>
        /// Delete Listening
        /// </summary>
        /// <param name="request">request.</param>
        Task RemoveListenerAsync(RemoveListenerRequest request);

        /// <summary>
        /// Get server status
        /// </summary>
        /// <returns>string</returns>
        Task<string> GetServerStatus();
    }
}
