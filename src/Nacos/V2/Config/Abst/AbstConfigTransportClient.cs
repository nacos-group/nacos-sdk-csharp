namespace Nacos.V2.Config.Abst
{
    using Nacos.V2.Common;
    using Nacos.V2.Config.Impl;
    using Nacos.V2.Security;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class AbstConfigTransportClient : IConfigTransportClient
    {
        protected NacosSdkOptions _options;
        protected ServerListManager _serverListManager;
        protected ISecurityProxy _securityProxy;
        protected string _accessKey;
        protected string _secretKey;

        public string GetName() => GetNameInner();

        public string GetNamespace() => GetNamespaceInner();

        public string GetTenant() => GetTenantInner();

        public Task<bool> PublishConfigAsync(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content)
            => PublishConfig(dataId, group, tenant, appName, tag, betaIps, content);

        public Task<List<string>> QueryConfigAsync(string dataId, string group, string tenat, long readTimeous, bool notify)
            => QueryConfig(dataId, group, tenat, readTimeous, notify);

        public Task<bool> RemoveConfigAsync(string dataId, string group, string tenat, string tag)
            => RemoveConfig(dataId, group, tenat, tag);

        public void Start() => StartInner();

        protected abstract string GetNameInner();

        protected abstract string GetNamespaceInner();

        protected abstract string GetTenantInner();

        protected abstract Task<bool> PublishConfig(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content);

        protected abstract Task<bool> RemoveConfig(string dataId, string group, string tenat, string tag);

        protected abstract Task<List<string>> QueryConfig(string dataId, string group, string tenat, long readTimeous, bool notify);

        protected abstract Task RemoveCache(string dataId, string group);

        protected abstract void StartInner();

        public Task RemoveCacheAsync(string dataId, string group) => RemoveCache(dataId, group);

        protected abstract Task ExecuteConfigListen();

        public Task ExecuteConfigListenAsync() => ExecuteConfigListen();

        protected Dictionary<string, string> GetSpasHeaders()
        {
            var spasHeaders = new Dictionary<string, string>(2);

            // STS 临时凭证鉴权的优先级高于 AK/SK 鉴权
            // StsConfig.getInstance().isStsOn()
            if (true)
            {
                // StsCredential stsCredential = getStsCredential();
                // _accessKey = stsCredential.accessKeyId;
                // _secretKey = stsCredential.accessKeySecret;
                // stsCredential.securityToken
                spasHeaders["Spas-SecurityToken"] = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(_accessKey) && !string.IsNullOrWhiteSpace(_secretKey))
            {
                spasHeaders["Spas-AccessKey"] = _accessKey;
            }

            return spasHeaders;
        }

        protected Dictionary<string, string> GetSecurityHeaders()
        {
            if (string.IsNullOrWhiteSpace(_securityProxy.GetAccessToken())) return null;

            var spasHeaders = new Dictionary<string, string>(2);
            spasHeaders[Constants.ACCESS_TOKEN] = _securityProxy.GetAccessToken();
            return spasHeaders;
        }

        protected Dictionary<string, string> GetCommonHeader()
        {
            var headers = new Dictionary<string, string>(16);

            string ts = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            var appKey = Environment.GetEnvironmentVariable("nacos.client.appKey");
            string token = HashUtil.GetMd5(ts + (string.IsNullOrWhiteSpace(appKey) ? "" : appKey));

            headers[Constants.CLIENT_APPNAME_HEADER] = AppDomain.CurrentDomain.FriendlyName;
            headers[Constants.CLIENT_REQUEST_TS_HEADER] = ts;
            headers[Constants.CLIENT_REQUEST_TOKEN_HEADER] = token;
            headers[HttpHeaderConsts.CLIENT_VERSION_HEADER] = Constants.CLIENT_VERSION;
            headers["exConfigInfo"] = "true";
            headers[HttpHeaderConsts.REQUEST_ID] = Guid.NewGuid().ToString("N");
            headers[HttpHeaderConsts.ACCEPT_CHARSET] = "UTF-8";
            return headers;
        }
    }
}
