namespace Nacos.V2.Config.Impl
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Common;
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Config.FilterImpl;
    using Nacos.V2.Config.Http;
    using Nacos.V2.Config.Utils;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConfigHttpTransportClient : AbstConfigTransportClient
    {
        private static readonly long POST_TIMEOUT = 3000L;

        private readonly ILogger _logger;

        private ConcurrentDictionary<string, CacheData> _cacheMap;

        private ConcurrentDictionary<int, Timer> _longPollingMap = new ConcurrentDictionary<int, Timer>();

        private IHttpAgent _agent;

        private double _currentLongingTaskCount = 0;
        private long _timeout;
        private int _taskPenaltyTime;

        private Timer _executeConfigListenTimer;

        public ConfigHttpTransportClient(
            ILogger logger,
            NacosSdkOptions options,
            ServerListManager serverListManager,
            ConcurrentDictionary<string, CacheData> cacheMap)
        {
            this._logger = logger;
            this._options = options;
            this._serverListManager = serverListManager;
            this._cacheMap = cacheMap;
            this._securityProxy = new Security.SecurityProxy(options);
            Init();

            _agent = new ServerHttpAgent(_logger, options);

            StartInner();
        }

        private void Init()
        {
            _timeout = Constants.CONFIG_LONG_POLL_TIMEOUT;
            _taskPenaltyTime = Constants.CONFIG_RETRY_TIME;
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
                    if (!_longPollingMap.ContainsKey(i))
                    {
                        var t = LongPolling(i);
                        _longPollingMap.TryAdd(i, t);
                    }
                }

                _currentLongingTaskCount = longingTaskCount;
            }

            return Task.CompletedTask;
        }

        private async Task<List<string>> GetServerConfig(string dataId, string group, string tenant, long readTimeout, bool notify)
        {
            if (group.IsNullOrWhiteSpace()) group = Constants.DEFAULT_GROUP;

            return await QueryConfig(dataId, group, tenant, readTimeout, notify);
        }

        private async Task<List<string>> CheckUpdateDataIds(List<CacheData> cacheDatas, List<string> inInitializingCacheList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (CacheData cacheData in cacheDatas)
            {
                if (!cacheData.IsUseLocalConfig)
                {
                    sb.Append(cacheData.DataId).Append(Constants.WORD_SEPARATOR);
                    sb.Append(cacheData.Group).Append(Constants.WORD_SEPARATOR);
                    if (cacheData.Tenant.IsNullOrWhiteSpace())
                    {
                        sb.Append(cacheData.Md5).Append(Constants.LINE_SEPARATOR);
                    }
                    else
                    {
                        sb.Append(cacheData.Md5).Append(Constants.WORD_SEPARATOR);
                        sb.Append(cacheData.Tenant).Append(Constants.LINE_SEPARATOR);
                    }

                    if (cacheData.IsInitializing)
                    {
                        // It updates when cacheData occours in cacheMap by first time.
                        inInitializingCacheList
                                .Add(GroupKey.GetKeyTenant(cacheData.DataId, cacheData.Group, cacheData.Tenant));
                    }
                }
            }

            var isInitializingCacheList = inInitializingCacheList != null && inInitializingCacheList.Any();
            return await CheckUpdateConfigStr(sb.ToString(), isInitializingCacheList);
        }

        private async Task<List<string>> CheckUpdateConfigStr(string probeUpdateString, bool isInitializingCacheList)
        {
            var parameters = new Dictionary<string, string>(2)
            {
                [Constants.PROBE_MODIFY_REQUEST] = probeUpdateString
            };

            var headers = new Dictionary<string, string>(2);
            headers["Long-Pulling-Timeout"] = _timeout.ToString();

            // told server do not hang me up if new initializing cacheData added in
            if (isInitializingCacheList)
            {
                headers["Long-Pulling-Timeout-No-Hangup"] = "true";
            }

            if (probeUpdateString.IsNullOrWhiteSpace()) return new List<string>();

            try
            {
                AssembleHttpParams(parameters, headers);

                // In order to prevent the server from handling the delay of the client's long task,
                // increase the client's read timeout to avoid this problem.
                long readTimeoutMs = _timeout + (long)Math.Round((_timeout >> 1) * 1d);

                var result = await _agent.HttpPost(Constants.CONFIG_CONTROLLER_PATH + "/listener", headers, parameters, "", readTimeoutMs);

                if (result.IsSuccessStatusCode)
                {
                    _isHealthServer = true;
                    var data = await result.Content.ReadAsStringAsync();
                    return ParseUpdateDataIdResponse(data);
                }
                else
                {
                    _isHealthServer = false;
                    _logger?.LogError("[{0}] [check-update] get changed dataId error, code: {1}", _agent.GetName(), result.StatusCode.ToString());
                }
            }
            catch (Exception ex)
            {
                _isHealthServer = false;
                _logger?.LogError(ex, "[{0}] [check-update] get changed dataId exception", _agent.GetName());
                throw;
            }

            return null;
        }

        private List<string> ParseUpdateDataIdResponse(string response)
        {
            if (string.IsNullOrEmpty(response)) return new List<string>();

            response = System.Net.WebUtility.UrlDecode(response);

            var updateList = new List<string>();

            foreach (string dataIdAndGroup in response.SplitByString(Constants.LINE_SEPARATOR))
            {
                if (!string.IsNullOrEmpty(dataIdAndGroup))
                {
                    var keyArr = dataIdAndGroup.SplitByString(Constants.WORD_SEPARATOR);
                    if (keyArr.Length < 2)
                        continue;
                    string dataId = keyArr[0];
                    string group = keyArr[1];
                    if (keyArr.Length == 2)
                    {
                        updateList.Add(GroupKey.GetKey(dataId, group));

                        _logger?.LogInformation(
                            "[{0}] [polling-resp] config changed. dataId={1}, group={2}",
                            _agent.GetName(), dataId, group);
                    }
                    else if (keyArr.Length == 3)
                    {
                        string tenant = keyArr[2];
                        updateList.Add(GroupKey.GetKeyTenant(dataId, group, tenant));

                        _logger?.LogInformation(
                            "[{0}] [polling-resp] config changed. dataId={1}, group={2}, tenant={3}",
                            _agent.GetName(), dataId, group, tenant);
                    }
                    else
                    {
                        _logger?.LogError("[{0}] [polling-resp] invalid dataIdAndGroup error {1}", _agent.GetName(),
                            dataIdAndGroup);
                    }
                }
            }

            return updateList;
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

            if (tenant.IsNotNullOrWhiteSpace()) parameters["tenant"] = tenant;
            if (appName.IsNotNullOrWhiteSpace()) parameters["appName"] = appName;
            if (tag.IsNotNullOrWhiteSpace()) parameters["tag"] = tag;

            var headers = new Dictionary<string, string>(1);
            if (betaIps.IsNotNullOrWhiteSpace()) headers["betaIps"] = betaIps;

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
            if (group.IsNullOrWhiteSpace()) group = Constants.DEFAULT_GROUP;

            HttpResponseMessage result = null;
            try
            {
                var paramters = new Dictionary<string, string>(3);
                if (tenant.IsNullOrWhiteSpace())
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

                        if (t.IsNotNullOrWhiteSpace()) ct[1] = t;
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
                            "[{0}] [sub-server-error] get server config being modified concurrently, dataId={1}, group={2}, tenant={3}",
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
                foreach (var item in securityHeaders) paramValues[item.Key] = item.Value;

                if (_options.Namespace.IsNotNullOrWhiteSpace()
                    && !paramValues.ContainsKey("tenant"))
                {
                    paramValues["tenant"] = _options.Namespace;
                }
            }

            var spasHeaders = GetSpasHeaders();
            if (spasHeaders != null)
            {
                foreach (var item in spasHeaders) headers[item.Key] = item.Value;
            }

            var commonHeader = GetCommonHeader();
            if (commonHeader != null)
            {
                foreach (var item in commonHeader) headers[item.Key] = item.Value;
            }

            var signHeaders = GetSignHeaders(paramValues, _options.SecretKey);
            if (signHeaders != null)
            {
                foreach (var item in signHeaders) headers[item.Key] = item.Value;
            }
        }

        protected override Task RemoveCache(string dataId, string group) => Task.CompletedTask;

        protected override async Task<bool> RemoveConfig(string dataId, string group, string tenant, string tag)
        {
            group = ParamUtils.Null2DefaultGroup(group);
            ParamUtils.CheckKeyParam(dataId, group);
            string url = Constants.CONFIG_CONTROLLER_PATH;
            var parameters = new Dictionary<string, string>(4)
            {
                ["dataId"] = dataId,
                ["group"] = group
            };

            if (tenant.IsNotNullOrWhiteSpace()) parameters["tenant"] = tenant;
            if (tag.IsNotNullOrWhiteSpace()) parameters["tag"] = tag;

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
                   "[{0}] [remove] error,, dataId={1}, group={2}, tenant={3}, code={4}, msg={5}",
                   _agent.GetName(), dataId, group, tenant, (int)result.StatusCode, result.StatusCode.ToString());
                throw new NacosException((int)result.StatusCode, result.StatusCode.ToString());
            }
            else
            {
                _logger?.LogWarning(
                   "[{0}] [remove] error,, dataId={1}, group={2}, tenant={3}, code={4}, msg={5}",
                   _agent.GetName(), dataId, group, tenant, (int)result.StatusCode, result.StatusCode.ToString());
                return false;
            }
        }

        protected override void StartInner()
        {
            _executeConfigListenTimer = new Timer(
                   async x =>
                   {
                       try
                       {
                           await ExecuteConfigListen();
                       }
                       catch (Exception ex)
                       {
                           _logger?.LogError(ex, "[{0}] [sub-check] rotate check error", _agent.GetName());
                       }

                       if (_executeConfigListenTimer != null)
                       {
                           _executeConfigListenTimer.Change(100, Timeout.Infinite);
                       }
                   }, null, 1, Timeout.Infinite);
        }

        protected override Task NotifyListenConfig() => Task.CompletedTask;

        private async Task CheckLocalConfig(string agentName, CacheData cacheData)
        {
            string dataId = cacheData.DataId;
            string group = cacheData.Group;
            string tenant = cacheData.Tenant;

            var path = FileLocalConfigInfoProcessor.GetFailoverFile(agentName, dataId, group, tenant);

            if (!cacheData.IsUseLocalConfig && path.Exists)
            {
                string content = await FileLocalConfigInfoProcessor.GetFailoverAsync(agentName, dataId, group, tenant);
                string md5 = HashUtil.GetMd5(content);
                cacheData.SetUseLocalConfigInfo(true);
                cacheData.SetLocalConfigInfoVersion(ObjectUtil.DateTimeToTimestamp(path.LastWriteTimeUtc));
                cacheData.SetContent(content);

                _logger?.LogWarning(
                    "[{0}] [failover-change] failover file created. dataId={1}, group={2}, tenant={3}, md5={4}, content={5}",
                    agentName, dataId, group, tenant, md5, ContentUtils.TruncateContent(content));

                return;
            }

            // If use local config info, then it doesn't notify business listener and notify after getting from server.
            if (cacheData.IsUseLocalConfig && !path.Exists)
            {
                cacheData.SetUseLocalConfigInfo(false);

                _logger?.LogWarning(
                  "[{0}] [failover-change] failover file deleted. dataId={1}, group={2}, tenant={3}",
                  agentName, dataId, group, tenant);
                return;
            }

            // When it changed.
            if (cacheData.IsUseLocalConfig
                && path.Exists
                && cacheData.GetLocalConfigInfoVersion() != ObjectUtil.DateTimeToTimestamp(path.LastWriteTimeUtc))
            {
                string content = await FileLocalConfigInfoProcessor.GetFailoverAsync(agentName, dataId, group, tenant);
                string md5 = HashUtil.GetMd5(content);
                cacheData.SetUseLocalConfigInfo(true);
                cacheData.SetLocalConfigInfoVersion(ObjectUtil.DateTimeToTimestamp(path.LastWriteTimeUtc));
                cacheData.SetContent(content);

                _logger?.LogWarning(
                   "[{0}] [failover-change] failover file created. dataId={1}, group={2}, tenant={3}, md5={4}, content={5}",
                   agentName, dataId, group, tenant, md5, ContentUtils.TruncateContent(content));
            }
        }

        private Timer LongPolling(int taskId)
        {
            return new Timer(
                async x =>
            {
                var cacheDatas = new List<CacheData>();
                var inInitializingCacheList = new List<string>();
                int selfTaskId = -1;
                int.TryParse(x.ToString(), out selfTaskId);
                try
                {
                    foreach (var cacheData in _cacheMap.Values)
                    {
                        // no listener should stop here.
                        if (cacheData.TaskId == selfTaskId && cacheData.GetListeners() != null && cacheData.GetListeners().Any())
                        {
                            cacheDatas.Add(cacheData);
                            try
                            {
                                await CheckLocalConfig(_agent.GetName(), cacheData);

                                if (cacheData.IsUseLocalConfig) cacheData.CheckListenerMd5();
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError(ex, "get local config info error");
                            }
                        }
                    }

                    var changedGroupKeys = await CheckUpdateDataIds(cacheDatas, inInitializingCacheList);

                    foreach (string groupKey in changedGroupKeys)
                    {
                        string[] key = GroupKey.ParseKey(groupKey);
                        string dataId = key[0];
                        string group = key[1];
                        string tenant = null;
                        if (key.Length == 3) tenant = key[2];

                        try
                        {
                            List<string> ct = await GetServerConfig(dataId, group, tenant, 3000L, true);

                            if (_cacheMap.TryGetValue(GroupKey.GetKeyTenant(dataId, group, tenant), out var cache))
                            {
                                cache.SetContent(ct[0]);

                                if (ct[1] != null) cache.Type = ct[1];

                                _logger?.LogInformation("[{0}] [data-received] dataId={1}, group={2}, tenant={3}, md5={4}, content={5}, type={6}", _agent.GetName(), dataId, group, tenant, cache.Md5,
                                       ContentUtils.TruncateContent(ct[0]), ct[1]);
                            }
                        }
                        catch (Exception ioe)
                        {
                            _logger?.LogError(ioe, "[{0}] [get-update] get changed config exception. dataId={0}, group={2}, tenant={3}", _agent.GetName(), dataId, group, tenant);
                        }
                    }

                    foreach (var cacheData in cacheDatas)
                    {
                        if (!cacheData.IsInitializing || inInitializingCacheList.Contains(GroupKey.GetKeyTenant(cacheData.DataId, cacheData.Group, cacheData.Tenant)))
                        {
                            cacheData.CheckListenerMd5();
                            cacheData.IsInitializing = false;
                        }
                    }

                    inInitializingCacheList.Clear();

                    if (_longPollingMap.TryGetValue(selfTaskId, out var t)) t.Change(1, Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "longPolling error");
                    if (_longPollingMap.TryGetValue(selfTaskId, out var t))
                    {
                        t.Change(_taskPenaltyTime, Timeout.Infinite);
                    }
                }
            }, taskId, 0, Timeout.Infinite);
        }
    }
}
