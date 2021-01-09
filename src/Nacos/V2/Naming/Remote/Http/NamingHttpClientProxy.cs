namespace Nacos.V2.Naming.Remote.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Common;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Naming.Beat;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Core;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Utils;
    using Nacos.V2.Remote;
    using Nacos.V2.Security;
    using Nacos.Utilities;
    using Microsoft.Extensions.Options;
    using Nacos.V2.Utils;

    public class NamingHttpClientProxy : INamingClientProxy
    {
        private static readonly int DEFAULT_SERVER_PORT = 8848;

        private static readonly string NAMING_SERVER_PORT = "nacos.naming.exposed.port";

        private ILogger _logger;

        private readonly IHttpClientFactory _clientFactory;

        private string namespaceId;

        private SecurityProxy _securityProxy;

        private long _securityInfoRefreshIntervalMills = 5000;

        private ServerListManager serverListManager;

        private BeatReactor beatReactor;

        private ServiceInfoHolder serviceInfoHolder;

        private PushReceiver pushReceiver;

        private int serverPort = DEFAULT_SERVER_PORT;

        private NacosSdkOptions _options;

        private Timer _loginTimer;

        public NamingHttpClientProxy(ILogger logger, string namespaceId, ServerListManager serverListManager, IOptionsMonitor<NacosSdkOptions> options, ServiceInfoHolder serviceInfoHolder, IHttpClientFactory clientFactory = null)
        {
            this._logger = logger;
            this._clientFactory = clientFactory;
            this.serverListManager = serverListManager;
            this._securityProxy = new SecurityProxy(options);
            this._options = options.CurrentValue;
            this.SetServerPort(DEFAULT_SERVER_PORT);
            this.namespaceId = namespaceId;
            this.beatReactor = new BeatReactor(_logger, this, _options);
            this.InitRefreshTask();
            this.pushReceiver = new PushReceiver(serviceInfoHolder);
            this.serviceInfoHolder = serviceInfoHolder;
        }

        internal async Task<Newtonsoft.Json.Linq.JObject> SendBeat(BeatInfo beatInfo, bool lightBeatEnabled)
        {
            var parameters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, beatInfo.ServiceName },
                { CommonParams.CLUSTER_NAME, beatInfo.Cluster },
                { "ip", beatInfo.Ip.ToString() },
                { "port", beatInfo.Port.ToString() },
            };

            var body = new Dictionary<string, string>();

            if (!lightBeatEnabled) body["beat"] = beatInfo.ToJsonString();

            var result = await ReqApi(UtilAndComs.NacosUrlBase + "/instance/beat", parameters, body, HttpMethod.Put);
            return Newtonsoft.Json.Linq.JObject.Parse(result);
        }

        private void SetServerPort(int serverPort)
        {
            this.serverPort = serverPort;

            // env first
            var env = EnvUtil.GetEnvValue(NAMING_SERVER_PORT);
            if (!string.IsNullOrWhiteSpace(env) && int.TryParse(env, out var port))
            {
                this.serverPort = port;
            }
        }

        private void InitRefreshTask()
        {
            _loginTimer = new Timer(
                 async x =>
                 {
                     await _securityProxy.LoginAsync(serverListManager.GetServerList());
                 }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_securityInfoRefreshIntervalMills));

            _securityProxy.LoginAsync(serverListManager.GetServerList()).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task CreateService(Service service, AbstractSelector selector)
        {
            _logger?.LogInformation("[CREATE-SERVICE] {0} creating service : {1} ", namespaceId, service);

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, service.Name },
                { CommonParams.GROUP_NAME, service.GroupName },
                { "protectThreshold", service.ProtectThreshold.ToString() },
                { "metadata", service.Metadata.ToJsonString() },
                { "selector", selector.ToJsonString() },
            };

            await ReqApi(UtilAndComs.NacosUrlService, paramters, HttpMethod.Post);
        }

        public async Task<bool> DeleteService(string serviceName, string groupName)
        {
            _logger?.LogInformation("[DELETE-SERVICE] {0} deleting service : {1} with groupName : {2} ", namespaceId, serviceName, groupName);

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, serviceName },
                { CommonParams.GROUP_NAME, groupName },
            };

            var result = await ReqApi(UtilAndComs.NacosUrlService, paramters, HttpMethod.Delete);
            return "ok".Equals(result);
        }

        public async Task DeregisterService(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[REGISTER-SERVICE] {0} deregistering service {1} with instance: {2}", namespaceId, serviceName, instance);

            string groupedServiceName = NamingUtils.GetGroupedName(serviceName, groupName);

            if (instance.Ephemeral)
            {
                beatReactor.RemoveBeatInfo(groupedServiceName, instance.Ip, instance.Port);
            }

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, groupedServiceName },
                { CommonParams.GROUP_NAME, groupName },
                { CommonParams.CLUSTER_NAME, instance.ClusterName },
                { "ip", instance.Ip },
                { "port", instance.Port.ToString() },
                { "ephemeral", instance.Ephemeral.ToString() },
            };

            await ReqApi(UtilAndComs.NacosUrlInstance, paramters, HttpMethod.Delete);
        }

        public async Task<ListView<string>> GetServiceList(int pageNo, int pageSize, string groupName, AbstractSelector selector)
        {
            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.GROUP_NAME, groupName },
                { "pageNo", pageNo.ToString() },
                { "pageSize", pageSize.ToString() },
            };

            if (selector != null && selector.Type.Equals("label"))
            {
                paramters["selector"] = selector.ToJsonString();
            }

            var result = await ReqApi(UtilAndComs.NacosUrlBase + "/service/list", paramters, HttpMethod.Get);

            var json = Newtonsoft.Json.Linq.JObject.Parse(result);
            var count = json.GetValue("count")?.ToObject<int>() ?? 0;
            var data = json.GetValue("doms")?.ToObject<List<string>>() ?? new List<string>();

            ListView<string> listView = new ListView<string>(count, data);
            return listView;
        }

        public async Task<ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, int udpPort, bool healthyOnly)
        {
            string groupedServiceName = NamingUtils.GetGroupedName(serviceName, groupName);

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, groupedServiceName },
                { CommonParams.CLUSTER_NAME, clusters },
                { "udpPort", udpPort.ToString() },

                // TODO:
                { "clientIP", "127.0.0.1" },
                { "healthyOnly", healthyOnly.ToString() },
            };

            var result = await ReqApi(UtilAndComs.NacosUrlBase + "/instance/list", paramters, HttpMethod.Get);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result.ToObj<ServiceInfo>();
            }

            return new ServiceInfo(groupedServiceName, clusters);
        }

        public async Task<Service> QueryService(string serviceName, string groupName)
        {
            _logger?.LogInformation("[QUERY-SERVICE] {0} registering service {1} : {2}", namespaceId, serviceName, groupName);

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, serviceName },
                { CommonParams.GROUP_NAME, groupName },
            };


            var result = await ReqApi(UtilAndComs.NacosUrlService, paramters, HttpMethod.Get);
            return result.ToObj<Service>();
        }

        public async Task RegisterServiceAsync(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[REGISTER-SERVICE] {0} registering service {1} with instance: {2}", namespaceId, serviceName, instance);

            string groupedServiceName = NamingUtils.GetGroupedName(serviceName, groupName);
            if (instance.Ephemeral)
            {
                BeatInfo beatInfo = beatReactor.BuildBeatInfo(groupedServiceName, instance);
                beatReactor.AddBeatInfo(groupedServiceName, beatInfo);
            }

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, groupedServiceName },
                { CommonParams.GROUP_NAME, groupName },
                { CommonParams.CLUSTER_NAME, instance.ClusterName },
                { "ip", instance.Ip },
                { "port", instance.Port.ToString() },
                { "weight", instance.Weight.ToString() },
                { "enable", instance.Enabled.ToString() },
                { "healthy", instance.Healthy.ToString() },
                { "ephemeral", instance.Ephemeral.ToString() },
                { "metadata", instance.Metadata.ToJsonString() },
            };

            await ReqApi(UtilAndComs.NacosUrlInstance, paramters, HttpMethod.Post);
        }

        private async Task<string> ReqApi(string url, Dictionary<string, string> paramters, HttpMethod method)
            => await ReqApi(url, paramters, new Dictionary<string, string>(), method);

        private async Task<string> ReqApi(string url, Dictionary<string, string> paramters, Dictionary<string, string> body, HttpMethod method)
            => await ReqApi(url, paramters, body, serverListManager.GetServerList(), method);

        private async Task<string> ReqApi(string url, Dictionary<string, string> paramters, Dictionary<string, string> body, List<string> servers, HttpMethod method)
        {
            paramters[CommonParams.NAMESPACE_ID] = namespaceId;

            if ((servers == null || !servers.Any()) && serverListManager.IsDomain())
                throw new NacosException(NacosException.INVALID_PARAM, "no server available");

            NacosException exception = new NacosException(string.Empty);

            if (servers != null && servers.Any())
            {
                Random random = new Random((int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                int index = random.Next(servers.Count);

                for (int i = 0; i < servers.Count; i++)
                {
                    var server = servers[i];
                    try
                    {
                        return await CallServer(url, paramters, body, server, method);
                    }
                    catch (NacosException e)
                    {
                        exception = e;
                        _logger?.LogDebug(e, "request {0} failed.", server);
                    }

                    index = (index + 1) % servers.Count;
                }
            }

            if (serverListManager.IsDomain())
            {
                for (int i = 0; i < UtilAndComs.REQUEST_DOMAIN_RETRY_COUNT; i++)
                {
                    try
                    {
                        return await CallServer(url, paramters, body, serverListManager.GetNacosDomain(), method);
                    }
                    catch (NacosException e)
                    {
                        exception = e;
                        _logger?.LogDebug(e, "request {0} failed.", serverListManager.GetNacosDomain());
                    }
                }
            }

            _logger?.LogError("request: {0} failed, servers: {1}, code: {2}, msg: {3}", url, servers, exception.ErrorCode, exception.ErrorMsg);

            throw new NacosException(exception.ErrorCode, $"failed to req API: {url} after all servers({servers}) tried: {exception.ErrorMsg}");
        }

        private async Task<string> CallServer(string api, Dictionary<string, string> paramters, Dictionary<string, string> body, string curServer, HttpMethod method)
        {
            InjectSecurityInfo(paramters);

            var headers = NamingHttpUtil.BuildHeader();

            var url = string.Empty;

            if (curServer.StartsWith(UtilAndComs.HTTPS) || curServer.StartsWith(UtilAndComs.HTTP))
            {
                url = curServer + api;
            }
            else
            {
                if (IPUtil.ContainsPort(curServer))
                {
                    curServer = curServer + IPUtil.IP_PORT_SPLITER + serverPort;
                }

                // TODO http or https
                url = UtilAndComs.HTTP + curServer + api;
            }

            try
            {
                var client = _clientFactory?.CreateClient(ConstValue.ClientName) ?? new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(8);
                var requestUrl = $"{url}?{InitParams(paramters, body)}";
                var requestMessage = new HttpRequestMessage(method, requestUrl);

                BuildHeader(requestMessage, headers);

                var responseMessage = await client.SendAsync(requestMessage);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var content = await responseMessage.Content.ReadAsStringAsync();
                    return content;
                }
                else if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotModified)
                {
                    return string.Empty;
                }

                throw new NacosException((int)responseMessage.StatusCode, responseMessage.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[NA] failed to request");
                throw new NacosException(NacosException.SERVER_ERROR, ex.Message);
            }
        }

        private void InjectSecurityInfo(Dictionary<string, string> paramters)
        {
            if (!string.IsNullOrWhiteSpace(_securityProxy.GetAccessToken()))
            {
                paramters[Constants.ACCESS_TOKEN] = _securityProxy.GetAccessToken();
            }

            paramters["app"] = AppDomain.CurrentDomain.FriendlyName;
            if (string.IsNullOrWhiteSpace(_options.AccessKey)
                && string.IsNullOrWhiteSpace(_options.SecretKey))
                return;

            string signData = string.IsNullOrWhiteSpace(paramters["serviceName"])
                ? DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "@@" + paramters["serviceName"]
                : DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

            string signature = Utilities.HashUtil.GetHMACSHA1(signData, _options.SecretKey);
            paramters["signature"] = signature;
            paramters["data"] = signData;
            paramters["ak"] = _options.AccessKey;
        }

        private void BuildHeader(HttpRequestMessage requestMessage, Dictionary<string, string> headers)
        {
            requestMessage.Headers.Clear();

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    requestMessage.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }
        }

        public bool ServerHealthy()
        {
            try
            {
                string result = ReqApi(UtilAndComs.NacosUrlBase + "/operator/metrics", new Dictionary<string, string>(),
                        HttpMethod.Get).ConfigureAwait(false).GetAwaiter().GetResult();

                var json = Newtonsoft.Json.Linq.JObject.Parse(result);

                string serverStatus = json.GetValue("status")?.ToString();
                return "UP".Equals(serverStatus);
            }
            catch
            {
                return false;
            }
        }

        public async Task<ServiceInfo> Subscribe(string serviceName, string groupName, string clusters)
        {
            return await QueryInstancesOfService(serviceName, groupName, clusters, 0, false);
        }

        public Task Unsubscribe(string serviceName, string groupName, string clusters)
        {
            return Task.CompletedTask;
        }

        public Task UpdateBeatInfo(List<Instance> modifiedInstances)
        {
            foreach (var instance in modifiedInstances)
            {
                string key = beatReactor.BuildKey(instance.ServiceName, instance.Ip, instance.Port);

                if (beatReactor.Dom2Beat.ContainsKey(key) && instance.Ephemeral)
                {
                    BeatInfo beatInfo = beatReactor.BuildBeatInfo(instance);
                    beatReactor.AddBeatInfo(instance.ServiceName, beatInfo);
                }
            }

            return Task.CompletedTask;
        }

        public async Task UpdateInstance(string serviceName, string groupName, Instance instance)
        {
            _logger?.LogInformation("[UPDATE-SERVICE] {0} update service {1} with instance: {2}", namespaceId, serviceName, instance);

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, serviceName },
                { CommonParams.GROUP_NAME, groupName },
                { CommonParams.CLUSTER_NAME, instance.ClusterName },
                { "ip", instance.Ip },
                { "port", instance.Port.ToString() },
                { "weight", instance.Weight.ToString() },
                { "enable", instance.Enabled.ToString() },
                { "ephemeral", instance.Ephemeral.ToString() },
                { "metadata", instance.Metadata.ToJsonString() },
            };

            await ReqApi(UtilAndComs.NacosUrlInstance, paramters, HttpMethod.Put);
        }

        public async Task UpdateService(Service service, AbstractSelector selector)
        {
            _logger?.LogInformation("[UPDATE-SERVICE] {0} updating service : {1} ", namespaceId, service);

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, service.Name },
                { CommonParams.GROUP_NAME, service.GroupName },
                { "protectThreshold", service.ProtectThreshold.ToString() },
                { "metadata", service.Metadata.ToJsonString() },
                { "selector", selector.ToJsonString() },
            };

            await ReqApi(UtilAndComs.NacosUrlService, paramters, HttpMethod.Put);
        }

        private string InitParams(Dictionary<string, string> dict, Dictionary<string, string> body)
        {
            var builder = new StringBuilder(1024);
            if (dict != null && dict.Any())
            {
                foreach (var item in dict)
                {
                    builder.Append($"{item.Key}={item.Value}&");
                }
            }

            if (body != null && body.Any())
            {
                foreach (var item in body)
                {
                    builder.Append($"{item.Key}={item.Value}&");
                }
            }

            return builder.ToString().TrimEnd('&');
        }
    }
}
