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
    using Nacos.V2.Utils;

    public class NamingHttpClientProxy : INamingClientProxy
    {
        private static readonly int DEFAULT_SERVER_PORT = 8848;

        private static readonly string NAMING_SERVER_PORT = "nacos.naming.exposed.port";

        private ILogger _logger;

        private readonly IHttpClientFactory _clientFactory;

        private string namespaceId;

        private SecurityProxy _securityProxy;

        private ServerListManager serverListManager;

        private BeatReactor beatReactor;

        private ServiceInfoHolder serviceInfoHolder;

        private PushReceiver pushReceiver;

        private int serverPort = DEFAULT_SERVER_PORT;

        private NacosSdkOptions _options;

        public NamingHttpClientProxy(
            ILogger logger,
            string namespaceId,
            SecurityProxy securityProxy,
            ServerListManager serverListManager,
            NacosSdkOptions options,
            ServiceInfoHolder serviceInfoHolder,
            IHttpClientFactory clientFactory = null)
        {
            this._logger = logger;
            this._clientFactory = clientFactory;
            this.serverListManager = serverListManager;
            this._securityProxy = securityProxy;
            this._options = options;
            this.SetServerPort(DEFAULT_SERVER_PORT);
            this.namespaceId = namespaceId;
            this.beatReactor = new BeatReactor(_logger, this, _options);

            // Don't create PushReceiver when using rpc, it will create a udp server
            if (!options.NamingUseRpc)
            {
                this.pushReceiver = new PushReceiver(_logger, serviceInfoHolder, _options);
            }

            this.serviceInfoHolder = serviceInfoHolder;
        }

        internal async Task<Newtonsoft.Json.Linq.JObject> SendBeat(BeatInfo beatInfo, bool lightBeatEnabled)
        {
            var parameters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, beatInfo.ServiceName },
                { CommonParams.CLUSTER_NAME, beatInfo.Cluster },
                { CommonParams.IP_PARAM, beatInfo.Ip.ToString() },
                { CommonParams.PORT_PARAM, beatInfo.Port.ToString() },
            };

            var body = new Dictionary<string, string>();

            if (!lightBeatEnabled) body["beat"] = beatInfo.ToJsonString();

            var result = await ReqApi(UtilAndComs.NacosUrlBase + "/instance/beat", parameters, body, HttpMethod.Put).ConfigureAwait(false);
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

        public async Task CreateService(Service service, AbstractSelector selector)
        {
            _logger?.LogInformation("[CREATE-SERVICE] {0} creating service : {1} ", namespaceId, service);

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, service.Name },
                { CommonParams.GROUP_NAME, service.GroupName },
                { CommonParams.PROTECT_THRESHOLD_PARAM, service.ProtectThreshold.ToString() },
                { CommonParams.META_PARAM, service.Metadata.ToJsonString() },
                { CommonParams.SELECTOR_PARAM, selector.ToJsonString() },
            };

            await ReqApi(UtilAndComs.NacosUrlService, paramters, HttpMethod.Post).ConfigureAwait(false);
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

            var result = await ReqApi(UtilAndComs.NacosUrlService, paramters, HttpMethod.Delete).ConfigureAwait(false);
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
                { CommonParams.IP_PARAM, instance.Ip },
                { CommonParams.PORT_PARAM, instance.Port.ToString() },
                { CommonParams.EPHEMERAL_PARAM, instance.Ephemeral.ToString() },
            };

            await ReqApi(UtilAndComs.NacosUrlInstance, paramters, HttpMethod.Delete).ConfigureAwait(false);
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
                paramters[CommonParams.SELECTOR_PARAM] = selector.ToJsonString();
            }

            var result = await ReqApi(UtilAndComs.NacosUrlBase + "/service/list", paramters, HttpMethod.Get).ConfigureAwait(false);

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
                { CommonParams.CLUSTERS_PARAM, clusters },
                { CommonParams.UDP_PORT_PARAM, udpPort.ToString() },
                { CommonParams.CLIENT_IP_PARAM, NetUtils.LocalIP() },
                { CommonParams.HEALTHY_ONLY_PARAM, healthyOnly.ToString() },
            };

            var result = await ReqApi(UtilAndComs.NacosUrlBase + "/instance/list", paramters, HttpMethod.Get).ConfigureAwait(false);
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

            var result = await ReqApi(UtilAndComs.NacosUrlService, paramters, HttpMethod.Get).ConfigureAwait(false);
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
                { CommonParams.IP_PARAM, instance.Ip },
                { CommonParams.PORT_PARAM, instance.Port.ToString() },
                { CommonParams.WEIGHT_PARAM, instance.Weight.ToString() },
                { CommonParams.ENABLE_PARAM, instance.Enabled.ToString() },
                { CommonParams.HEALTHY_PARAM, instance.Healthy.ToString() },
                { CommonParams.EPHEMERAL_PARAM, instance.Ephemeral.ToString() },
                { CommonParams.META_PARAM, instance.Metadata.ToJsonString() },
            };

            await ReqApi(UtilAndComs.NacosUrlInstance, paramters, HttpMethod.Post).ConfigureAwait(false);
        }

        private async Task<string> ReqApi(string url, Dictionary<string, string> paramters, HttpMethod method)
            => await ReqApi(url, paramters, new Dictionary<string, string>(), method).ConfigureAwait(false);

        private async Task<string> ReqApi(string url, Dictionary<string, string> paramters, Dictionary<string, string> body, HttpMethod method)
            => await ReqApi(url, paramters, body, serverListManager.GetServerList(), method).ConfigureAwait(false);

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
                        return await CallServer(url, paramters, body, server, method).ConfigureAwait(false);
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
                        return await CallServer(url, paramters, body, serverListManager.GetNacosDomain(), method).ConfigureAwait(false);
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
                url = curServer.TrimEnd('/') + api;
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
                var client = _clientFactory?.CreateClient(Nacos.V2.Common.Constants.ClientName) ?? new HttpClient();

                using var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(8000));

                var requestUrl = $"{url}?{InitParams(paramters, body)}";
                var requestMessage = new HttpRequestMessage(method, requestUrl);

                BuildHeader(requestMessage, headers);

                var responseMessage = await client.SendAsync(requestMessage, cts.Token).ConfigureAwait(false);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return content;
                }
                else if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotModified)
                {
                    return string.Empty;
                }

                // response body will contains some error message
                var msg = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                throw new NacosException((int)responseMessage.StatusCode, $"{responseMessage.StatusCode}--{msg}");
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

            paramters[CommonParams.APP_FILED] = AppDomain.CurrentDomain.FriendlyName;
            if (string.IsNullOrWhiteSpace(_options.AccessKey)
                || string.IsNullOrWhiteSpace(_options.SecretKey))
                return;

            string signData = !string.IsNullOrWhiteSpace(paramters[CommonParams.SERVICE_NAME_PARAM])
                ? DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + CommonParams.SEPARATOR + paramters[CommonParams.SERVICE_NAME_PARAM]
                : DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

            string signature = HashUtil.GetHMACSHA1(signData, _options.SecretKey);
            paramters[CommonParams.SIGNATURE_FILED] = signature;
            paramters[CommonParams.DATA_FILED] = signData;
            paramters[CommonParams.AK_FILED] = _options.AccessKey;
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
            return await QueryInstancesOfService(serviceName, groupName, clusters, pushReceiver.GetUdpPort(), false).ConfigureAwait(false);
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

                if (beatReactor._dom2Beat.ContainsKey(key) && instance.Ephemeral)
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
                { CommonParams.IP_PARAM, instance.Ip },
                { CommonParams.PORT_PARAM, instance.Port.ToString() },
                { CommonParams.WEIGHT_PARAM, instance.Weight.ToString() },
                { CommonParams.ENABLE_PARAM, instance.Enabled.ToString() },
                { CommonParams.EPHEMERAL_PARAM, instance.Ephemeral.ToString() },
                { CommonParams.META_PARAM, instance.Metadata.ToJsonString() },
            };

            await ReqApi(UtilAndComs.NacosUrlInstance, paramters, HttpMethod.Put).ConfigureAwait(false);
        }

        public async Task UpdateService(Service service, AbstractSelector selector)
        {
            _logger?.LogInformation("[UPDATE-SERVICE] {0} updating service : {1} ", namespaceId, service);

            var paramters = new Dictionary<string, string>()
            {
                { CommonParams.NAMESPACE_ID, namespaceId },
                { CommonParams.SERVICE_NAME, service.Name },
                { CommonParams.GROUP_NAME, service.GroupName },
                { CommonParams.PROTECT_THRESHOLD_PARAM, service.ProtectThreshold.ToString() },
                { CommonParams.META_PARAM, service.Metadata.ToJsonString() },
                { CommonParams.SELECTOR_PARAM, selector.ToJsonString() },
            };

            await ReqApi(UtilAndComs.NacosUrlService, paramters, HttpMethod.Put).ConfigureAwait(false);
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

        public Task<bool> IsSubscribed(string serviceName, string groupName, string clusters) => Task.FromResult(true);

        public Task BatchRegisterServiceAsync(string serviceName, string groupName, List<Instance> instances)
        {
            throw new NotImplementedException("Do not support persistent instances to perform batch registration methods.");
        }
    }
}
