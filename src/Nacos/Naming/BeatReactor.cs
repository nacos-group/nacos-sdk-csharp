namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.IO;
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
                    File.AppendAllText("Final.txt", ":" + "Timer is called" + System.Environment.NewLine);
                    await BeatTask(beatInfo, _timer);
                }, null, 0, 10000);

            return Task.CompletedTask;

            // MetricsMonitor.Dom2BeatSizeMonitor.set(Dom2Beat.Count);
        }

        private async Task BeatTask(BeatInfo beatInfo, Timer timer)
        {
            File.AppendAllText("Final.txt", ":" + "BeatInfo Flag: " + beatInfo.stopped.ToString() + System.Environment.NewLine);
            if (beatInfo.stopped == true)
            {
                timer.Dispose();
                return;
            }

            bool flag = false;
            try
            {
                // send heart beat will register instance
                File.AppendAllText("Final.txt", ":" + "Hearbeat sent" + System.Environment.NewLine);
                flag = await _namingClient.SendHeartbeatAsync(new SendHeartbeatRequest
                {
                    Ephemeral = true,
                    ServiceName = beatInfo.serviceName,
                    BeatInfo = beatInfo,
                });
                File.AppendAllText("Final.txt", ":" + "Flag:" + flag + System.Environment.NewLine);
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