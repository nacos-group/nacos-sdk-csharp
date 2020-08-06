namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
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
        protected INacosNamingClient _namingClient;
        private Timer _timer;
        public readonly IDictionary<string, BeatInfo> Dom2Beat = new ConcurrentDictionary<string, BeatInfo>();

        public BeatReactor(
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BeatReactor>();
        }

        /// <summary>
        /// Add beat information.
        /// </summary>
        /// <param name="serviceName"> service name </param>
        /// <param name="beatInfo">    beat information </param>
        public Task AddBeatInfo(string serviceName, BeatInfo beatInfo)
        {
            _logger.LogInformation("[BEAT] adding beat: {} to beat map.", beatInfo);
            string key = BuildKey(serviceName, beatInfo.ip, beatInfo.port);
            BeatInfo existBeat = null;
            Dom2Beat.TryGetValue(key, out existBeat);
            var del = Dom2Beat.Remove(key);
            if (existBeat != null)
            {
                existBeat.stopped = true;
            }

            Dom2Beat[key] = beatInfo;
            var count = 0;
            _timer = new Timer(
                async x =>
                {
                    count = count + 1;
                    if (count == 2)
                    {
                        _timer.Dispose();
                    }

                    await BeatTask(beatInfo);
                }, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));

            return Task.CompletedTask;

            // MetricsMonitor.Dom2BeatSizeMonitor.set(Dom2Beat.Count);
        }

        private async Task BeatTask(BeatInfo beatInfo)
        {
            bool flag = false;

            try
            {
                // send heart beat will register instance
                flag = await _namingClient.SendHeartbeatAsync(new SendHeartbeatRequest
                {
                    Ephemeral = true,
                    ServiceName = beatInfo.serviceName,
                    BeatInfo = beatInfo,
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Send heart beat to Nacos error");
            }

            _logger.LogDebug("report at {0}, status = {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), flag);
        }

        /// <summary>
        /// Remove beat information.
        /// </summary>
        /// <param name="serviceName"> service name </param>
        /// <param name="ip">          ip of beat information </param>
        /// <param name="port">        port of beat information </param>
        public void RemoveBeatInfo(string serviceName, string ip, int port)
        {
            _logger.LogInformation("[BEAT] removing beat: {}:{}:{} from beat map.", serviceName, ip, port);
            BeatInfo beatInfo = null;
            Dom2Beat.TryGetValue(BuildKey(serviceName, ip, port), out beatInfo);
            Dom2Beat.Remove(BuildKey(serviceName, ip, port));
            if (beatInfo == null)
            {
                return;
            }

            beatInfo.stopped = true;

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
        /// <param name="groupedServiceName"> service name with group name, format: ${groupName}@@${serviceName} </param>
        /// <param name="instance"> instance </param>
        /// <returns> new beat information </returns>
        public BeatInfo BuildBeatInfo(string groupedServiceName, RegisterInstanceRequest instance)
        {
            BeatInfo beatInfo = new BeatInfo();
            beatInfo.serviceName = groupedServiceName;
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
            return serviceName + "#" + ip + "#" + port;
        }
    }
}