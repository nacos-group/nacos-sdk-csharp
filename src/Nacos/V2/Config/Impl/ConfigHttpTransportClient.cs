namespace Nacos.V2.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Common;
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Config.FilterImpl;
    using Nacos.V2.Config.Http;
    using Nacos.V2.Config.Utils;
    using Nacos.V2.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConfigHttpTransportClient : AbstConfigTransportClient
    {
        private static readonly long POST_TIMEOUT = 3000L;

        private readonly ILogger _logger;

        private Dictionary<string, CacheData> _cacheMap;

        private IHttpAgent _agent;

        private double _currentLongingTaskCount = 0;

        private Timer _executeConfigListenTimer;

        public ConfigHttpTransportClient(
            ILogger logger,
            NacosSdkOptions options,
            ServerListManager serverListManager,
            Dictionary<string, CacheData> cacheMap)
        {
            this._logger = logger;
            this._options = options;
            this._serverListManager = serverListManager;
            this._cacheMap = cacheMap;

            _agent = new ServerHttpAgent(_logger, options);
        }


        protected override Task ExecuteConfigListen()
        {
            // Dispatch taskes.
            int listenerSize = _cacheMap.Count;

            // Round up the longingTaskCount.
            int longingTaskCount = (int)Math.Ceiling(listenerSize * 1.0 / CacheData.PerTaskConfigSize);
            if (longingTaskCount > _currentLongingTaskCount)
            {
                for (int i = (int)_currentLongingTaskCount; i < longingTaskCount; i++)
                {
                    // The task list is no order.So it maybe has issues when changing.
                    // executorService.execute(new LongPollingRunnable(agent, i, this));
                }

                _currentLongingTaskCount = longingTaskCount;
            }

            return Task.CompletedTask;
        }

        protected override string GetNameInner() => _agent.GetName();

        protected override string GetNamespaceInner() => _agent.GetNamespace();

        protected override string GetTenantInner() => _agent.GetTenant();

        protected override async Task<bool> PublishConfig(string dataId, string group, string tenant, string appName, string tag, string betaIps, string content)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            ParamUtils.CheckParam(dataId, group, content);

            ConfigRequest cr = new ConfigRequest();
            cr.SetDataId(dataId);
            cr.SetTenant(tenant);
            cr.SetGroup(group);
            cr.SetContent(content);

            // _configFilterChainManager.doFilter(cr, null);
            content = cr.GetContent();

            string url = Constants.CONFIG_CONTROLLER_PATH;

            var parameters = new Dictionary<string, string>(6);
            parameters["dataId"] = dataId;
            parameters["group"] = group;
            parameters["content"] = content;

            if (!string.IsNullOrWhiteSpace(tenant)) parameters["tenant"] = tenant;
            if (!string.IsNullOrWhiteSpace(appName)) parameters["appName"] = appName;
            if (!string.IsNullOrWhiteSpace(tag)) parameters["tag"] = tag;

            var headers = new Dictionary<string, string>(1);
            if (!string.IsNullOrWhiteSpace(betaIps)) headers["betaIps"] = betaIps;

            HttpResponseMessage result = null;
            try
            {
                result = await HttpPost(url, headers, parameters, "", POST_TIMEOUT);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                  ex,
                  "[{0}] [publish-single] exception, dataId={1}, group={2}, tenant={3}",
                  _agent.GetName(), dataId, group, tenant);

                return false;
            }

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger?.LogInformation(
                 "[{0}] [publish-single] ok, dataId={1}, group={2}, tenant={3}, config={4}",
                 _agent.GetName(), dataId, group, tenant, ContentUtils.TruncateContent(content));

                return true;
            }
            else if (result.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger?.LogWarning(
                "[{0}] [publish-single] error, dataId={1}, group={2}, tenant={3}, code={4}, msg={5}",
                _agent.GetName(), dataId, group, tenant, (int)result.StatusCode, result.StatusCode.ToString());
                throw new NacosException((int)result.StatusCode, result.StatusCode.ToString());
            }
            else
            {
                _logger?.LogWarning(
               "[{0}] [publish-single] error, dataId={1}, group={2}, tenant={3}, code={4}, msg={5}",
               _agent.GetName(), dataId, group, tenant, (int)result.StatusCode, result.StatusCode.ToString());
                return false;
            }
        }

        protected override async Task<List<string>> QueryConfig(string dataId, string group, string tenant, long readTimeout, bool notify)
        {
            string[] ct = new string[2];
            if (string.IsNullOrWhiteSpace(group)) group = Constants.DEFAULT_GROUP;

            HttpResponseMessage result = null;
            try
            {
                var paramters = new Dictionary<string, string>(3);
                if (string.IsNullOrWhiteSpace(tenant))
                {
                    paramters["dataId"] = dataId;
                    paramters["group"] = group;
                }
                else
                {
                    paramters["dataId"] = dataId;
                    paramters["group"] = group;
                    paramters["tenant"] = tenant;
                }

                var headers = new Dictionary<string, string>(16);
                headers["notify"] = notify.ToString();

                result = await HttpGet(Constants.CONFIG_CONTROLLER_PATH, headers, paramters, _agent.GetEncode(), readTimeout);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "[{0}] [sub-server] get server config exception, dataId={1}, group={2}, tenant={3}",
                    _agent.GetName(), dataId, group, tenant);

                throw new NacosException(NacosException.SERVER_ERROR, ex.Message);
            }

            switch (result.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var content = await result.Content.ReadAsStringAsync();

                    await FileLocalConfigInfoProcessor.SaveSnapshotAsync(_agent.GetName(), dataId, group, tenant, content);
                    ct[0] = content;

                    if (result.Headers.TryGetValues(Constants.CONFIG_TYPE, out var values))
                    {
                        var t = values.FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(t)) ct[1] = t;
                        else ct[1] = "text";
                    }
                    else
                    {
                        ct[1] = "text";
                    }

                    return ct.ToList();
                case System.Net.HttpStatusCode.NotFound:
                    await FileLocalConfigInfoProcessor.SaveSnapshotAsync(_agent.GetName(), dataId, group, tenant, null);
                    return ct.ToList();
                case System.Net.HttpStatusCode.Conflict:
                    {
                        _logger?.LogError(
                            "[{}] [sub-server-error] get server config being modified concurrently, dataId={}, group={}, tenant={}",
                            _agent.GetName(), dataId, group, tenant);

                        throw new NacosException(NacosException.CONFLICT, "data being modified, dataId=" + dataId + ",group=" + group + ",tenant=" + tenant);
                    }

                case System.Net.HttpStatusCode.Forbidden:
                    {
                        _logger?.LogError(
                           "[{0}] [sub-server-error] no right, dataId={1}, group={2}, tenant={3}",
                           _agent.GetName(), dataId, group, tenant);

                        throw new NacosException((int)result.StatusCode, result.StatusCode.ToString());
                    }

                default:
                    {
                        _logger?.LogError(
                          "[{0}] [sub-server-error] , dataId={1}, group={2}, tenant={3}, code={4}",
                          _agent.GetName(), dataId, group, tenant, result.StatusCode);
                        throw new NacosException((int)result.StatusCode, "http error, code=" + (int)result.StatusCode + ",dataId=" + dataId + ",group=" + group + ",tenant=" + tenant);
                    }
            }
        }

        private async Task<HttpResponseMessage> HttpPost(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
        {
            if (headers == null) headers = new Dictionary<string, string>(16);

            AssembleHttpParams(paramValues, headers);
            return await _agent.HttpPost(path, headers, paramValues, encoding, readTimeoutMs);
        }

        private async Task<HttpResponseMessage> HttpGet(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
        {
            if (headers == null) headers = new Dictionary<string, string>(16);

            AssembleHttpParams(paramValues, headers);
            return await _agent.HttpGet(path, headers, paramValues, encoding, readTimeoutMs);
        }

        private async Task<HttpResponseMessage> HttpDelete(string path, Dictionary<string, string> headers, Dictionary<string, string> paramValues, string encoding, long readTimeoutMs)
        {
            if (headers == null) headers = new Dictionary<string, string>(16);

            AssembleHttpParams(paramValues, headers);
            return await _agent.HttpDelete(path, headers, paramValues, encoding, readTimeoutMs);
        }

        private void AssembleHttpParams(Dictionary<string, string> paramValues, Dictionary<string, string> headers)
        {
            var securityHeaders = GetSecurityHeaders();

            if (securityHeaders != null)
            {
                // put security header to param
                foreach (var item in securityHeaders) paramValues[item.Key] = item.Value;

                if (!string.IsNullOrWhiteSpace(_options.Namespace)
                    && !paramValues.ContainsKey("tenant"))
                {
                    paramValues["tenant"] = _options.Namespace;
                }
            }

            var spasHeaders = GetSpasHeaders();
            if (spasHeaders != null)
            {
                // put spasHeader to header.
                foreach (var item in spasHeaders) headers[item.Key] = item.Value;
            }

            var commonHeader = GetCommonHeader();
            if (commonHeader != null)
            {
                // put common headers
                foreach (var item in commonHeader) headers[item.Key] = item.Value;
            }

            // SpasAdapter.getSignHeaders(params, super.secretKey);
            var signHeaders = new Dictionary<string, string>();
            if (signHeaders != null)
            {
                foreach (var item in signHeaders) headers[item.Key] = item.Value;
            }
        }

        protected override Task RemoveCache(string dataId, string group)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> RemoveConfig(string dataId, string group, string tenant, string tag)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            ParamUtils.CheckKeyParam(dataId, group);
            string url = Constants.CONFIG_CONTROLLER_PATH;
            var parameters = new Dictionary<string, string>(4);
            parameters["dataId"] = dataId;
            parameters["group"] = group;

            if (!string.IsNullOrWhiteSpace(tenant)) parameters["tenant"] = tenant;
            if (!string.IsNullOrWhiteSpace(tenant)) parameters["tag"] = tag;

            HttpResponseMessage result = null;
            try
            {
                result = await HttpDelete(url, null, parameters, "", POST_TIMEOUT);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                    ex,
                    "[remove] error,, dataId={1}, group={2}, tenant={3}",
                    dataId, group, tenant);
                return false;
            }

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger?.LogInformation(
                 "[{0}] [remove] ok, dataId={1}, group={2}, tenant={3}",
                 _agent.GetName(), dataId, group, tenant);

                return true;
            }
            else if (result.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger?.LogWarning(
                   "[{}] [remove] error,, dataId={1}, group={2}, tenant={3}, code={4}, msg={5}",
                   _agent.GetName(), dataId, group, tenant, (int)result.StatusCode, result.StatusCode.ToString());
                throw new NacosException((int)result.StatusCode, result.StatusCode.ToString());
            }
            else
            {
                _logger?.LogWarning(
                   "[{}] [remove] error,, dataId={1}, group={2}, tenant={3}, code={4}, msg={5}",
                   _agent.GetName(), dataId, group, tenant, (int)result.StatusCode, result.StatusCode.ToString());
                return false;
            }
        }

        protected override void StartInner()
        {
            _executeConfigListenTimer = new Timer(
                   async x =>
                   {
                       await ExecuteConfigListen();
                   }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
        }
    }
}
