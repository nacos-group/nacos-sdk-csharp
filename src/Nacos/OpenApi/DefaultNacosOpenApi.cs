namespace Nacos.OpenApi
{
    using Microsoft.Extensions.Options;
    using Nacos.V2.Utils;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class DefaultNacosOpenApi : INacosOpenApi
    {
        private const string _namespacePath = "nacos/v1/console/namespaces";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Nacos.V2.NacosSdkOptions _options;

        public DefaultNacosOpenApi(IHttpClientFactory httpClientFactory, IOptions<Nacos.V2.NacosSdkOptions> optionsAccs)
        {
            this._httpClientFactory = httpClientFactory;
            this._options = optionsAccs.Value;
        }

        public async Task<bool> CreateNamespaceAsync(string customNamespaceId, string namespaceName, string namespaceDesc)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClientName);

            var content = new StringContent($"customNamespaceId={customNamespaceId}&namespaceName={namespaceName}&namespaceDesc={namespaceDesc}");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var req = new HttpRequestMessage(HttpMethod.Post, $"{_options.ServerAddresses.First().TrimEnd('/')}/{_namespacePath}");
            req.Content = content;

            var resp = await client.SendAsync(req).ConfigureAwait(false);

            if (resp.IsSuccessStatusCode)
            {
                var res = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                bool.TryParse(res, out bool result);
                return result;
            }
            else
            {
                throw new Nacos.V2.Exceptions.NacosException((int)resp.StatusCode, "CreateNamespaceAsync exception");
            }
        }

        public async Task<bool> DeleteNamespaceAsync(string namespaceId)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClientName);

            var content = new StringContent($"namespaceId={namespaceId}");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var req = new HttpRequestMessage(HttpMethod.Delete, $"{_options.ServerAddresses.First().TrimEnd('/')}/{_namespacePath}");
            req.Content = content;

            var resp = await client.SendAsync(req).ConfigureAwait(false);

            if (resp.IsSuccessStatusCode)
            {
                var res = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                bool.TryParse(res, out bool result);
                return result;
            }
            else
            {
                throw new Nacos.V2.Exceptions.NacosException((int)resp.StatusCode, "DeleteNamespaceAsync exception");
            }
        }

        public async Task<List<NacosNamespace>> GetNamespacesAsync()
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClientName);

            var req = new HttpRequestMessage(HttpMethod.Get, $"{_options.ServerAddresses.First().TrimEnd('/')}/{_namespacePath}");

            var resp = await client.SendAsync(req).ConfigureAwait(false);

            if (resp.IsSuccessStatusCode)
            {
                var res = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var jobj = Newtonsoft.Json.Linq.JObject.Parse(res);

                if (jobj.TryGetValue("data", System.StringComparison.OrdinalIgnoreCase, out var val))
                {
                    return val.ToString().ToObj<List<NacosNamespace>>();
                }
                else
                {
                    return new List<NacosNamespace>();
                }
            }
            else
            {
                throw new Nacos.V2.Exceptions.NacosException((int)resp.StatusCode, "GetNamespacesAsync exception");
            }
        }

        public async Task<bool> UpdateNamespaceAsync(string namespaceId, string namespaceName, string namespaceDesc)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClientName);

            var content = new StringContent($"customNamespaceId={namespaceId}&namespaceName={namespaceName}&namespaceDesc={namespaceDesc}");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var req = new HttpRequestMessage(HttpMethod.Post, $"{_options.ServerAddresses.First().TrimEnd('/')}/{_namespacePath}");
            req.Content = content;

            var resp = await client.SendAsync(req).ConfigureAwait(false);

            if (resp.IsSuccessStatusCode)
            {
                var res = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                bool.TryParse(res, out bool result);
                return result;
            }
            else
            {
                throw new Nacos.V2.Exceptions.NacosException((int)resp.StatusCode, "UpdateNamespaceAsync exception");
            }
        }
    }
}
