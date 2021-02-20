namespace Nacos.V2.Config.Http
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IHttpAgent : IDisposable
    {
        Task Start();

        Task<HttpResponseMessage> HttpGet(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs);

        Task<HttpResponseMessage> HttpPost(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs);

        Task<HttpResponseMessage> HttpDelete(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs);

        string GetName();

        string GetNamespace();

        string GetTenant();

        string GetEncode();
    }
}
