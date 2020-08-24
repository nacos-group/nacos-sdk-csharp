namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Nacos.Exceptions;
    using Nacos.Utilities;

    /// <summary>
    /// Beat reactor.
    /// </summary>
    public class BeatReactor
    {
        private readonly ILogger _logger;
        private readonly NacosOptions _options;
        private readonly Nacos.Naming.Http.NamingProxy _proxy;
        private Timer _timer;
        public readonly IDictionary<string, BeatInfo> Dom2Beat = new ConcurrentDictionary<string, BeatInfo>();

        public BeatReactor(
            ILoggerFactory loggerFactory,
            Nacos.Naming.Http.NamingProxy proxy,
            NacosOptions optionAccs)
        {
            _logger = loggerFactory.CreateLogger<BeatReactor>();
            _proxy = proxy;
            _options = optionAccs;
        }

        /// <summary>
        /// Add beat information.
        /// </summary>
        /// <param name="serviceName"> service name </param>
        /// <param name="beatInfo">    beat information </param>
        public Task AddBeatInfo(string serviceName, BeatInfo beatInfo)
        {
            _logger.LogInformation("[BEAT] adding beat: {0} to beat map.", beatInfo.ToJsonString());
            string key = BuildKey(serviceName, beatInfo.ip, beatInfo.port);
            BeatInfo existBeat = null;
            Dom2Beat.TryGetValue(key, out existBeat);
            var del = Dom2Beat.Remove(key);
            if (existBeat != null)
            {
                existBeat.stopped = true;
            }

            Dom2Beat[key] = beatInfo;
            _timer = new Timer(
                async x =>
                {
                    await BeatTask(beatInfo, _timer);
                }, null, 0, 10000);

            return Task.CompletedTask;

            // MetricsMonitor.Dom2BeatSizeMonitor.set(Dom2Beat.Count);
        }

        private async Task BeatTask(BeatInfo beatInfo, Timer timer)
        {
            if (beatInfo.stopped == true)
            {
                timer.Dispose();
                return;
            }

            try
            {
                // send heart beat will register instance
                var request = new SendHeartbeatRequest
                {
                    Ephemeral = true,
                    ServiceName = beatInfo.serviceName,
                    BeatInfo = beatInfo,
                };

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
                        }
                        else
                        {
                            _logger.LogWarning($"[client.SendHeartbeat] server return {result} ");
                        }

                        break;
                    default:
                        _logger.LogWarning($"[client.SendHeartbeat] Send instance beat failed {responseMessage.StatusCode.ToString()}");
                        throw new NacosException((int)responseMessage.StatusCode, $"Send instance beat failed {responseMessage.StatusCode.ToString()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Send heart beat to Nacos error");
            }
        }

        /// <summary>
        /// Remove beat information.
        /// </summary>
        /// <param name="serviceName"> service name </param>
        /// <param name="ip">          ip of beat information </param>
        /// <param name="port">        port of beat information </param>
        public Task RemoveBeatInfo(string serviceName, string ip, int port)
        {
            _logger.LogInformation("[BEAT] removing beat: {0}:{1}:{2} from beat map.", serviceName, ip, port);
            BeatInfo beatInfo = null;
            Dom2Beat.TryGetValue(BuildKey(serviceName, ip, port), out beatInfo);
            Dom2Beat.Remove(BuildKey(serviceName, ip, port));
            if (beatInfo != null)
            {
                beatInfo.stopped = true;
            }

            return Task.CompletedTask;

            // MetricsMonitor.Dom2BeatSizeMonitor.set(Dom2Beat.Count);
        }

        /// <summary>
        /// Build new beat information.
        /// </summary>
        /// <param name="instance"> instance </param>
        /// <returns> new beat information </returns>
        public BeatInfo BuildBeatInfo(RegisterInstanceRequest instance)
        {
            return BuildBeatInfo(instance.ServiceName, instance);
        }

        /// <summary>
        /// Build new beat information.
        /// </summary>
        /// <param name="serviceName"> service name with group name, format: ${groupName}@@${serviceName} </param>
        /// <param name="instance"> instance </param>
        /// <returns> new beat information </returns>
        public BeatInfo BuildBeatInfo(string serviceName, RegisterInstanceRequest instance)
        {
            BeatInfo beatInfo = new BeatInfo();
            beatInfo.serviceName = serviceName;
            beatInfo.ip = instance.Ip;
            beatInfo.port = instance.Port;
            beatInfo.cluster = instance.ClusterName;
            beatInfo.weight = instance.Weight;
            beatInfo.metadata = instance.Metadata;
            beatInfo.scheduled = false;
            beatInfo.period = 5;
            return beatInfo;
        }

        public string BuildKey(string serviceName, string ip, int port)
        {
            return serviceName + ConstValue.BeatInfoSplitter + ip + ConstValue.BeatInfoSplitter + port;
        }
    }
}