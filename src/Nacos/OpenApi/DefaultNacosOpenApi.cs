namespace Nacos.OpenApi
{
    using Microsoft.Extensions.Options;
    using Nacos.Utils;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class DefaultNacosOpenApi : INacosOpenApi
    {
        private const string _namespacePath = "nacos/v1/console/namespaces";
        private const string _metricsPath = "nacos/v1/ns/operator/metrics";

        private readonly IHttpClientFactory _httpClientFactory;

/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
        private readonly Nacos.NacosSdkOptions _options;
在此之后:
        private readonly NacosSdkOptions _options;
*/
        private readonly Nacos.NacosSdkOptions _options;


/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
        public DefaultNacosOpenApi(IHttpClientFactory httpClientFactory, IOptions<Nacos.NacosSdkOptions> optionsAccs)
在此之后:
        public DefaultNacosOpenApi(IHttpClientFactory httpClientFactory, IOptions<NacosSdkOptions> optionsAccs)
*/
        public DefaultNacosOpenApi(IHttpClientFactory httpClientFactory, IOptions<Nacos.NacosSdkOptions> optionsAccs)
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
                throw new Nacos.Exceptions.NacosException((int)resp.StatusCode, "CreateNamespaceAsync exception");
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
                throw new Nacos.Exceptions.NacosException((int)resp.StatusCode, "DeleteNamespaceAsync exception");
            }
        }

        public async Task<NacosMetrics> GetMetricsAsync(bool onlyStatus)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClientName);

            var req = new HttpRequestMessage(HttpMethod.Get, $"{_options.ServerAddresses.First().TrimEnd('/')}/{_metricsPath}?onlyStatus={onlyStatus.ToString().ToLower()}");

            var resp = await client.SendAsync(req).ConfigureAwait(false);

            if (resp.IsSuccessStatusCode)
            {
                var res = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                return res.ToObj<NacosMetrics>();
            }
            else
            {
                throw new Nacos.Exceptions.NacosException((int)resp.StatusCode, "GetMetricsAsync exception");
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
                var jobj = System.Text.Json.Nodes.JsonNode.Parse(res).AsObject();

                if (jobj.TryGetPropertyValue("data", out var val))
                {
                    return val.GetValue<List<NacosNamespace>>();
                }
                else
                {
                    return new List<NacosNamespace>();
                }
            }
            else
            {
                throw new Nacos.Exceptions.NacosException((int)resp.StatusCode, "GetNamespacesAsync exception");
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
                throw new Nacos.Exceptions.NacosException((int)resp.StatusCode, "UpdateNamespaceAsync exception");
            }
        }
    }
}
