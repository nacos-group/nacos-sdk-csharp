namespace Nacos.Config.Http
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public interface IHttpAgent
    {
        /// <summary>
        /// Get
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="headers">headers</param>
        /// <param name="paramValues">paramValues</param>
        /// <param name="timeout">timeout</param>
        Task<HttpResponseMessage> GetAsync(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, int timeout = 8000);

        /// <summary>
        /// Post
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="headers">headers</param>
        /// <param name="paramValues">paramValues</param>
        /// <param name="timeout">timeout</param>
        Task<HttpResponseMessage> PostAsync(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, int timeout = 8000);

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="headers">headers</param>
        /// <param name="paramValues">paramValues</param>
        /// <param name="timeout">timeout</param>
        Task<HttpResponseMessage> DeleteAsync(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, int timeout = 8000);

        /// <summary>
        /// get name
        /// </summary>
        string GetName();

        /// <summary>
        /// get namespace
        /// </summary>
        /// <returns>Namespace</returns>
        string GetNamespace();

        /// <summary>
        /// get tenant
        /// </summary>
        /// <returns>Tenant</returns>
        string GetTenant();
    }
}
