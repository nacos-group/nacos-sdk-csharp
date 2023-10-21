﻿namespace Nacos.Naming.Remote.Http
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos;
    using Nacos.Common;
    using Nacos.Exceptions;
    using Nacos.Logging;
    using Nacos.Naming.Dtos;
    using Nacos.Naming.Remote;
    using Nacos.Naming.Utils;
    using Nacos.Remote;
    using Nacos.Security;
    using Nacos.Utils;

    public class NamingHttpClientProxy : INamingHttpClientProxy
    {
        private static readonly int DEFAULT_SERVER_PORT = 8848;

        private static readonly string NAMING_SERVER_PORT = "nacos.naming.exposed.port";

        private ILogger _logger = NacosLogManager.CreateLogger<NamingHttpClientProxy>();

        private readonly IHttpClientFactory _clientFactory;

        private string namespaceId;

        private ISecurityProxy _securityProxy;

        private IServerListFactory serverListManager;

        private int serverPort = DEFAULT_SERVER_PORT;

        private NacosSdkOptions _options;

        public NamingHttpClientProxy(
            ILoggerFactory loggerFactory,
            ISecurityProxy securityProxy,
            IServerListFactory serverListManager,
            IOptions<NacosSdkOptions> optionsAccs,
            IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            this.serverListManager = serverListManager;
            _securityProxy = securityProxy;
            _options = optionsAccs.Value;
            SetServerPort(DEFAULT_SERVER_PORT);
            this.namespaceId = string.IsNullOrWhiteSpace(_options.Namespace) ? Constants.DEFAULT_NAMESPACE_ID : _options.Namespace;
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
                return;
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

            var json = System.Text.Json.Nodes.JsonNode.Parse(result).AsObject();
            var count = json["count"]?.GetValue<int>() ?? 0;
            var data = json["doms"]?.GetValue<List<string>>() ?? new List<string>();

            ListView<string> listView = new ListView<string>(count, data);
            return listView;
        }

        public Task<ServiceInfo> QueryInstancesOfService(string serviceName, string groupName, string clusters, bool healthyOnly)
        {
            throw new NotSupportedException("Do not support query instance by http client,please use gRPC replaced.");
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
                throw new NotSupportedException("Do not support register ephemeral instances by HTTP, please use gRPC replaced.");
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

            if (servers == null || !servers.Any())
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
                var client = _clientFactory?.CreateClient(Constants.ClientName) ?? new HttpClient();

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
            var result = GetSecurityHeaders(paramters[CommonParams.NAMESPACE_ID], paramters[CommonParams.GROUP_NAME], paramters[CommonParams.SERVICE_NAME]);

            foreach (var item in result)
            {
                paramters.Add(item.Key, item.Value);
            }
        }

        private Dictionary<string, string> GetSecurityHeaders(string @namespace, string group, string serviceName)
        {
            var resource = new Nacos.Auth.RequestResource("naming", @namespace, group, serviceName);
            var result = _securityProxy.GetIdentityContext(resource);
            result[CommonParams.APP_FILED] = AppDomain.CurrentDomain.FriendlyName;
            return result;
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

                var json = System.Text.Json.Nodes.JsonNode.Parse(result).AsObject();

                string serverStatus = json["status"]?.GetValue<string>();
                return "UP".Equals(serverStatus);
            }
            catch
            {
                return false;
            }
        }

        public Task<ServiceInfo> Subscribe(string serviceName, string groupName, string clusters)
        {
            throw new NotSupportedException("Do not support subscribe service by UDP, please use gRPC replaced.");
        }

        public Task Unsubscribe(string serviceName, string groupName, string clusters)
        {
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

        public void Dispose()
        {
        }

        public Task BatchDeregisterServiceAsync(string serviceName, string groupName, List<Instance> instances)
        {
            throw new NotImplementedException("Do not support persistent instances to perform batch de registration methods.");
        }
    }
}
