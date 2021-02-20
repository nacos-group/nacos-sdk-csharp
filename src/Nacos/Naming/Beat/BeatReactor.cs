namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Nacos.Exceptions;
    using Nacos.Utilities;
    using System;
    using System.Collections.Concurrent;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Beat reactor.
    /// </summary>
    public class BeatReactor
    {
        private readonly ILogger _logger;
        private readonly NacosOptions _options;
        private readonly Nacos.Naming.Http.NamingProxy _proxy;

        private readonly ConcurrentDictionary<string, BeatInfo> _dom2Beat = new ConcurrentDictionary<string, BeatInfo>();
        private readonly ConcurrentDictionary<string, Timer> _beatTimer = new ConcurrentDictionary<string, Timer>();

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
        /// <param name="serviceName">service name </param>
        /// <param name="beatInfo">beat information </param>
        public Task AddBeatInfo(string serviceName, BeatInfo beatInfo)
        {
            _logger.LogInformation("[BEAT] adding beat: {0} to beat map.", beatInfo.ToJsonString());
            string key = BuildKey(serviceName, beatInfo.ip, beatInfo.port);

            if (_dom2Beat.TryRemove(key, out var existBeat))
            {
                existBeat.stopped = true;
            }

            _dom2Beat.AddOrUpdate(key, beatInfo, (x, y) => beatInfo);

            var timer = new Timer(
                async x =>
                {
                    var info = x as BeatInfo;
                    await BeatTask(info);
                }, beatInfo, beatInfo.period, beatInfo.period);

            _beatTimer.AddOrUpdate(key, timer, (x, y) => timer);

            return Task.CompletedTask;
        }

        private async Task BeatTask(BeatInfo beatInfo)
        {
            if (beatInfo.stopped) return;

            try
            {
                // send heart beat will register instance
                var request = new SendHeartbeatRequest
                {
                    Ephemeral = true,
                    ServiceName = beatInfo.serviceName,
                    BeatInfo = beatInfo,
                    NameSpaceId = _options.Namespace,
                };

                if (request == null) throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "request param invalid");

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

                            if (!flag) _logger.LogWarning($"[CLIENT-BEAT] server return {result} ");
                        }
                        else
                        {
                            _logger.LogWarning($"[CLIENT-BEAT] server return {result} ");
                        }

                        break;
                    default:
                        _logger.LogWarning("[CLIENT-BEAT] failed to send beat {0}, {1}", beatInfo.ToJsonString(), responseMessage.StatusCode.ToString());
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
        /// <param name="serviceName">service name</param>
        /// <param name="ip">ip of beat information</param>
        /// <param name="port">port of beat information</param>
        public Task RemoveBeatInfo(string serviceName, string ip, int port)
        {
            _logger.LogInformation("[BEAT] removing beat: {0}:{1}:{2} from beat map.", serviceName, ip, port);

            var key = BuildKey(serviceName, ip, port);

            if (_dom2Beat.TryRemove(key, out var info))
            {
                info.stopped = true;
            }

            if (_beatTimer.TryRemove(key, out var t))
            {
                t.Dispose();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Build new beat information.
        /// </summary>
        /// <param name="serviceName">service name with group name, format: ${groupName}@@${serviceName}</param>
        /// <param name="instance">instance</param>
        /// <returns>new beat information</returns>
        public BeatInfo BuildBeatInfo(string serviceName, RegisterInstanceRequest instance)
        {
            BeatInfo beatInfo = new BeatInfo
            {
                serviceName = serviceName,
                ip = instance.Ip,
                port = instance.Port,
                cluster = instance.ClusterName,
                weight = instance.Weight,
                metadata = instance.Metadata,
                scheduled = false,

                // using the default value at first, the unit is ms.
                period = 5000
            };
            return beatInfo;
        }

        public string BuildKey(string serviceName, string ip, int port)
        {
            return serviceName + ConstValue.BeatInfoSplitter + ip + ConstValue.BeatInfoSplitter + port;
        }
    }
}