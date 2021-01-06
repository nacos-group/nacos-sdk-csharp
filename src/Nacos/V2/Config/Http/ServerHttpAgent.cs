namespace Nacos.V2.Config.Http
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ServerHttpAgent : IHttpAgent
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetEncode()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public string GetNamespace()
        {
            throw new NotImplementedException();
        }

        public string GetTenant()
        {
            throw new NotImplementedException();
        }

        public Task<string> HttpDelete(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
        {
            throw new NotImplementedException();
        }

        public Task<string> HttpGet(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
        {
            throw new NotImplementedException();
        }

        public Task<string> HttpPost(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
        {
            throw new NotImplementedException();
        }

        public Task Start()
        {
            throw new NotImplementedException();
        }
    }
}
