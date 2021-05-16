namespace Nacos.Config.Http
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class HttpAgent : IHttpAgent
    {
        public async Task<HttpResponseMessage> DeleteAsync(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, int timeout = 8000)
        {
            using var cts = new CancellationTokenSource(timeout);

            return await ReqApiAsync(HttpMethod.Delete, path, headers, paramValues, cts.Token).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> GetAsync(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, int timeout = 8000)
        {
            using var cts = new CancellationTokenSource(timeout);

            return await ReqApiAsync(HttpMethod.Get, path, headers, paramValues, cts.Token).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, int timeout = 8000)
        {
            using var cts = new CancellationTokenSource(timeout);

            return await ReqApiAsync(HttpMethod.Post, path, headers, paramValues, cts.Token).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, CancellationToken cancellationToken)
            => await ReqApiAsync(HttpMethod.Post, path, headers, paramValues, cancellationToken).ConfigureAwait(false);

        public string GetName() => AbstGetName();

        public string GetNamespace() => AbstGetNamespace();

        public string GetTenant() => AbstGetTenant();

        public abstract Task<HttpResponseMessage> ReqApiAsync(HttpMethod httpMethod, string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, CancellationToken cancellationToken);

        public abstract string AbstGetName();

        public abstract string AbstGetNamespace();

        public abstract string AbstGetTenant();
    }
}
