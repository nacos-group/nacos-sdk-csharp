namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Nacos.Exceptions;
    using Nacos.Utilities;
    using System;
    using System.IO;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class NacosNamingClient : INacosNamingClient
    {
        private readonly ILogger _logger;
        private readonly NacosOptions _options;
        private readonly Nacos.Naming.Http.NamingProxy _proxy;
        public BeatReactor _beatReactor;

        public HostReactor _hostReactor;

        private volatile bool closed = false;

        private volatile bool flag = false;

        public event Action MyEvent;

        public bool Count = false;
        public readonly ConcurrentDictionary<string, List<Listener>> ObserverMap = new ConcurrentDictionary<string, List<Listener>>();
        public readonly BlockingCollection<ServiceInfo> ChangedServices = new BlockingCollection<ServiceInfo>(boundedCapacity: 10);

        public NacosNamingClient(
            ILoggerFactory loggerFactory,
            IOptionsMonitor<NacosOptions> optionAccs,
            IHttpClientFactory clientFactory)
        {
            _logger = loggerFactory.CreateLogger<NacosNamingClient>();
            _options = optionAccs.CurrentValue;
            _proxy = new Naming.Http.NamingProxy(loggerFactory, _options, clientFactory);
            _beatReactor = new BeatReactor(loggerFactory);
            _hostReactor = new HostReactor(loggerFactory, _options);
            new Task(Running).Start();
        }

        #region Instance
        public async Task<bool> RegisterInstanceAsync(RegisterInstanceRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Post, RequestPathValue.INSTANCE, null, request.ToDict(), _options.DefaultTimeOut);

            if (request.Ephemeral == true)
            {
                string groupedServiceName;
                if (String.IsNullOrEmpty(request.GroupName))
                {
                    groupedServiceName = request.GroupName + "@@" + request.ServiceName;
                }
                else
                {
                    groupedServiceName = "DEFAULT_GROUP" + "@@" + request.ServiceName;
                }

                BeatInfo beatInfo = _beatReactor.BuildBeatInfo(groupedServiceName, request);

                await _beatReactor.AddBeatInfo(request.ServiceName, beatInfo);
            }

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine("Hey" + result);
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.RegisterInstance] server return {result} ");
                        return false;
                    }

                default:
                    _logger.LogWarning($"[client.RegisterInstance] Register an instance to service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Register an instance to service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<bool> RemoveInstanceAsync(RemoveInstanceRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Delete, RequestPathValue.INSTANCE, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.RemoveInstance] server return {result} ");
                        return false;
                    }

                default:
                    _logger.LogWarning($"[client.RemoveInstance] Delete instance from service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Delete instance from service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<bool> ModifyInstanceAsync(ModifyInstanceRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Put, RequestPathValue.INSTANCE, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.ModifyInstance] server return {result} ");
                        return false;
                    }

                default:
                    _logger.LogWarning($"[client.ModifyInstance] Modify an instance of service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Modify an instance of service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<ListInstancesResult> ListInstancesAsync(ListInstancesRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.INSTANCE_LIST, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var obj = result.ToObj<ListInstancesResult>();
                    return obj;
                default:
                    _logger.LogWarning($"[client.ListInstances] Query instance list of service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Query instance list of service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<GetInstanceResult> GetInstanceAsync(GetInstanceRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.INSTANCE, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var obj = result.ToObj<GetInstanceResult>();
                    return obj;
                default:
                    _logger.LogWarning($"[client.GetInstance] Query instance details of service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Query instance details of service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<bool> SendHeartbeatAsync(SendHeartbeatRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Put, RequestPathValue.INSTANCE_BEAT, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var jObj = Newtonsoft.Json.Linq.JObject.Parse(result);

                    if (jObj.ContainsKey("code"))
                    {
                        int code = int.Parse(jObj["code"].ToString());

                        var flag = code == 10200;

                        if (!flag) _logger.LogWarning($"[client.SendHeartbeat] server return {result} ");

                        return flag;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.SendHeartbeat] server return {result} ");
                        return false;
                    }

                default:
                    _logger.LogWarning($"[client.SendHeartbeat] Send instance beat failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Send instance beat failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<bool> ModifyInstanceHealthStatusAsync(ModifyInstanceHealthStatusRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Put, RequestPathValue.INSTANCE_HEALTH, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.ModifyInstanceHealthStatus] server return {result} ");
                        return false;
                    }

                case System.Net.HttpStatusCode.BadRequest:
                    _logger.LogWarning($"[client.ModifyInstanceHealthStatus] health check is still working {responseMessage.StatusCode.ToString()}");
                    return false;
                default:
                    _logger.LogWarning($"[client.ModifyInstanceHealthStatus] Update instance health status failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Update instance health status failed {responseMessage.StatusCode.ToString()}");
            }
        }
        #endregion

        #region Metrics
        public async Task<GetMetricsResult> GetMetricsAsync()
        {
            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.METRICS, null, new System.Collections.Generic.Dictionary<string, string>(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var obj = result.ToObj<GetMetricsResult>();
                    return obj;
                default:
                    _logger.LogWarning($"[client.GetMetrics] Query system metrics failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Query system metrics failed {responseMessage.StatusCode.ToString()}");
            }
        }
        #endregion

        #region Services
        public async Task<bool> CreateServiceAsync(CreateServiceRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Post, RequestPathValue.SERVICE, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.CreateService] server return {result} ");
                        return false;
                    }

                default:
                    _logger.LogWarning($"[client.CreateService] Create service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Create service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<bool> RemoveServiceAsync(RemoveServiceRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Delete, RequestPathValue.SERVICE, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.RemoveService] server return {result} ");
                        return false;
                    }

                default:
                    _logger.LogWarning($"[client.RemoveService] Delete a service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Delete a service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<bool> ModifyServiceAsync(ModifyServiceRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Put, RequestPathValue.SERVICE, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.ModifyService] server return {result} ");
                        return false;
                    }

                default:
                    _logger.LogWarning($"[client.ModifyService] Update a service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Update a service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<GetServiceResult> GetServiceAsync(GetServiceRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.SERVICE, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var obj = result.ToObj<GetServiceResult>();
                    return obj;
                default:
                    _logger.LogWarning($"[client.GetService] Query a service failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Query a service failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<ListServicesResult> ListServicesAsync(ListServicesRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.SERVICE_LIST, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var obj = result.ToObj<ListServicesResult>();
                    return obj;
                default:
                    _logger.LogWarning($"[client.ListServices] Query service list failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Query service list failed {responseMessage.StatusCode.ToString()}");
            }
        }
        #endregion

        #region Switches
        public async Task<GetSwitchesResult> GetSwitchesAsync()
        {
            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.SWITCHES, null, new System.Collections.Generic.Dictionary<string, string>(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var obj = result.ToObj<GetSwitchesResult>();
                    return obj;
                default:
                    _logger.LogWarning($"[client.GetSwitches] Query system switches failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Query system switches failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<bool> ModifySwitchesAsync(ModifySwitchesRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Put, RequestPathValue.SWITCHES, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    if (result.Equals("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning($"[client.ModifySwitches] server return {result} ");
                        return false;
                    }

                default:
                    _logger.LogWarning($"[client.ModifySwitches] Update system switch failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Update system switch failed {responseMessage.StatusCode.ToString()}");
            }
        }

        #endregion

        #region Cluster
        public async Task<ListClusterServersResult> ListClusterServersAsync(ListClusterServersRequest request)
        {
            if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

            request.CheckParam();

            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.SERVERS, null, request.ToDict(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var obj = result.ToObj<ListClusterServersResult>();
                    return obj;
                default:
                    _logger.LogWarning($"[client.ListClusterServers] Query server list failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"Query server list failed {responseMessage.StatusCode.ToString()}");
            }
        }

        public async Task<GetCurrentClusterLeaderResult> GetCurrentClusterLeaderAsync()
        {
            var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.LEADER, null, new System.Collections.Generic.Dictionary<string, string>(), _options.DefaultTimeOut);

            switch (responseMessage.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    var leader = result.GetPropValue("leader");
                    var obj = leader.ToObj<GetCurrentClusterLeaderResult>();
                    return obj;
                default:
                    _logger.LogWarning($"[client.GetCurrentClusterLeader] query the leader of current cluster failed {responseMessage.StatusCode.ToString()}");
                    throw new NacosException((int)responseMessage.StatusCode, $"query the leader of current cluster failed {responseMessage.StatusCode.ToString()}");
            }
        }
        #endregion

        public async Task<bool> AddListenerAsync(ServiceInfo serviceInfo, string clusters, Listener listener)
        {
            _logger.LogInformation("[LISTENER] adding " + serviceInfo.name + " with " + clusters + " to listener map");
            List<Listener> observers = new List<Listener>();

            observers.Add(listener);
            string name = ServiceInfo.getKey(serviceInfo.name, clusters);
            ObserverMap.AddOrUpdate(name, observers, (string name, List<Listener> observers) => observers);
            while (!Count)
            {
                var request = new ListInstancesRequest
                {
                    ServiceName = serviceInfo.name,
                    Callbacks = new List<Action<string>>
                    {
                        x => { Console.WriteLine(x); }
                    }
                };
                ChangedServices.Add(serviceInfo);
                await Notifier(request);
            }

            return true;
        }

        public Task ServiceIfChanged(string result)
        {
            var obj = result.ToObj<ServiceInfo>();
            ServiceInfo oldService = null;
            _hostReactor.ServiceInfoMap.TryGetValue(obj.getKey(), out oldService);
            if (obj.Hosts == null)
            {
                flag = false;
                return Task.CompletedTask;
            }

            if (oldService != null)
            {
                _hostReactor.ServiceInfoMap[obj.getKey()] = obj;
                _hostReactor.ServiceInfoMap.TryGetValue(obj.getKey(), out oldService);
                IDictionary<string, Host> oldHostMap = new ConcurrentDictionary<string, Host>();
                foreach (var entry in oldService.Hosts)
                {
                    oldHostMap[entry.ToInetAddr()] = entry;
                }

                IDictionary<string, Host> newHostMap = new ConcurrentDictionary<string, Host>();
                foreach (var entry in oldService.Hosts)
                {
                    newHostMap[entry.ToInetAddr()] = entry;
                }

                foreach (KeyValuePair<string, Host> entry in newHostMap)
                {
                    if (!oldHostMap.ContainsKey(entry.Key))
                    {
                        ChangedServices.Add(obj);
                        File.AppendAllText("output.txt", "Service Changed" + System.Environment.NewLine);
                        flag = true;
                    }
                    else
                    {
                        Host host1 = newHostMap[entry.Key];
                        Host host2 = oldHostMap[entry.Key];
                        if (host1.String() == host2.String())
                        {
                            flag = false;
                        }
                        else
                        {
                            ChangedServices.Add(obj);
                            File.AppendAllText("output.txt", "Service Changed" + System.Environment.NewLine);
                            flag = true;
                            return Task.CompletedTask;
                        }
                    }
                }

                foreach (KeyValuePair<string, Host> entry in oldHostMap)
                {
                    if (!newHostMap.ContainsKey(entry.Key))
                    {
                        File.AppendAllText("output.txt", "Service Changed" + System.Environment.NewLine);
                        ChangedServices.Add(obj);
                        flag = true;
                        return Task.CompletedTask;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
                _hostReactor.ServiceInfoMap[obj.getKey()] = obj;
                flag = true;
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private string BuildName(string tenant, string group, string dataId)
        {
            return $"{tenant}-{group}-{dataId}";
        }

        private async Task Notifier(ListInstancesRequest request)
        {
            try
            {
                if (request == null) throw new NacosException(ConstValue.CLIENT_INVALID_PARAM, "request param invalid");

                request.CheckParam();

                var responseMessage = await _proxy.ReqApiAsync(HttpMethod.Get, RequestPathValue.INSTANCE_LIST, null, request.ToDict(), _options.DefaultTimeOut);

                switch (responseMessage.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        var result = await responseMessage.Content.ReadAsStringAsync();
                        await ServiceIfChanged(result);
                        if (flag)
                        {
                            if (MyEvent != null) MyEvent();
                        }

                        break;
                    default:
                        _logger.LogWarning($"[client.ListInstances] Query instance list of service failed {responseMessage.StatusCode.ToString()}");
                        throw new NacosException((int)responseMessage.StatusCode, $"Query instance list of service failed {responseMessage.StatusCode.ToString()}");
                }
            }
            catch (Exception ex)
            {
                // SetHealthServer(false);
                _logger.LogError(ex, "[listener] error");
            }
        }

        public void Running()
        {
            while (!closed)
            {
                ServiceInfo serviceInfo = null;
                serviceInfo = ChangedServices.Take();
                if (serviceInfo == null)
                {
                    continue;
                }

                try
                {
                    List<Listener> listeners = ObserverMap[serviceInfo.getKey()];
                    if (listeners.Count != 0)
                    {
                        foreach (Listener listener in listeners)
                        {
                            List<Host> hosts = serviceInfo.Hosts;
                            File.AppendAllText("Final.txt", "Listening the event");
                            MyEvent += () => Console.WriteLine("We're doing something!");

                            // listener.onEvent(new NamingEvent(serviceInfo.getName(), serviceInfo.getGroupName(),
                            //     serviceInfo.getClusters(), hosts));
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning("[NA] notify error for service: " + serviceInfo.name + ", clusters: " + serviceInfo.clusters, e);
                }
            }
        }
    }
}
