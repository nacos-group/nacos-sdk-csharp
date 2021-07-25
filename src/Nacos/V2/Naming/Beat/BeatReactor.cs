namespace Nacos.V2.Naming.Beat
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Common;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Remote.Http;
    using Nacos.V2.Naming.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class BeatReactor
    {
        private static readonly string CLIENT_BEAT_INTERVAL_FIELD = "clientBeatInterval";
        private static readonly int RESOURCE_NOT_FOUND = 20404;
        private static readonly int OK = 10200;

        private readonly ILogger _logger;
        private readonly NamingHttpClientProxy _serverProxy;
        public readonly ConcurrentDictionary<string, BeatInfo> _dom2Beat;
        private readonly ConcurrentDictionary<string, Timer> _beatTimer;

        public BeatReactor(ILogger logger, NamingHttpClientProxy serverProxy, NacosSdkOptions options)
        {
            this._logger = logger;
            this._serverProxy = serverProxy;
            this._dom2Beat = new ConcurrentDictionary<string, BeatInfo>();
            this._beatTimer = new ConcurrentDictionary<string, Timer>();
        }

        internal string BuildKey(string serviceName, string ip, int port)
        {
            return serviceName + Constants.NAMING_INSTANCE_ID_SPLITTER + ip + Constants.NAMING_INSTANCE_ID_SPLITTER + port;
        }

        internal BeatInfo BuildBeatInfo(Instance instance) => BuildBeatInfo(instance.ServiceName, instance);

        internal void AddBeatInfo(string serviceName, BeatInfo beatInfo)
        {
            _logger?.LogInformation("[BEAT] adding beat: {0} to beat map.", beatInfo);

            string key = BuildKey(serviceName, beatInfo.Ip, beatInfo.Port);

            if (_dom2Beat.TryRemove(key, out var exitBeat)) exitBeat.Stopped = true;

            _dom2Beat.AddOrUpdate(key, beatInfo, (x, y) => beatInfo);

            var timer = new Timer(
                async x =>
                {
                    var info = x as BeatInfo;
                    await BeatTask(info).ConfigureAwait(false);
                }, beatInfo, beatInfo.Period, beatInfo.Period);

            _beatTimer.AddOrUpdate(key, timer, (x, y) => timer);
        }

        private async Task BeatTask(BeatInfo beatInfo)
        {
            if (beatInfo.Stopped) return;

            long nextTime = beatInfo.Period;

            try
            {
                Newtonsoft.Json.Linq.JObject result = await _serverProxy.SendBeat(beatInfo, false).ConfigureAwait(false);

                long interval = result.GetValue(CLIENT_BEAT_INTERVAL_FIELD).ToObject<long>();

                bool lightBeatEnabled = false;

                if (result.ContainsKey(CommonParams.LIGHT_BEAT_ENABLED))
                {
                    lightBeatEnabled = result.GetValue(CommonParams.LIGHT_BEAT_ENABLED).ToObject<bool>();
                }

                if (interval > 0) nextTime = interval;

                int code = OK;

                if (result.ContainsKey(CommonParams.CODE)) code = result.GetValue(CommonParams.CODE).ToObject<int>();

                if (code == RESOURCE_NOT_FOUND)
                {
                    Instance instance = new Instance
                    {
                        Port = beatInfo.Port,
                        Ip = beatInfo.Ip,
                        Weight = beatInfo.Weight ?? 1,
                        Metadata = beatInfo.Metadata,
                        ClusterName = beatInfo.Cluster,
                        ServiceName = beatInfo.ServiceName,
                        Ephemeral = true,

                        // InstanceId = ""
                    };

                    try
                    {
                        await _serverProxy.RegisterServiceAsync(beatInfo.ServiceName, NamingUtils.GetGroupName(beatInfo.ServiceName), instance).ConfigureAwait(false);
                    }
                    catch
                    {
                    }
                }
            }
            catch (NacosException ex)
            {
                _logger?.LogError(ex, "[CLIENT-BEAT] failed to send beat: {0}, code: {1}, msg: {2}", beatInfo, ex.ErrorCode, ex.ErrorMsg);
            }
            catch (Exception unknownEx)
            {
                _logger?.LogError(unknownEx, "[CLIENT-BEAT] failed to send beat: {0}, unknown exception msg: {1}", beatInfo, unknownEx.Message);
            }

            string key = BuildKey(beatInfo.ServiceName, beatInfo.Ip, beatInfo.Port);

            if (_beatTimer.TryGetValue(key, out var timer))
                timer.Change(nextTime, Timeout.Infinite);
        }

        internal BeatInfo BuildBeatInfo(string groupedServiceName, Instance instance)
        {
            return new BeatInfo
            {
                ServiceName = groupedServiceName,
                Ip = instance.Ip,
                Port = instance.Port,
                Cluster = instance.ClusterName,
                Weight = instance.Weight,
                Metadata = instance.Metadata,
                Period = instance.GetInstanceHeartBeatInterval(),
                Scheduled = false
            };
        }

        internal void RemoveBeatInfo(string serviceName, string ip, int port)
        {
            _logger?.LogInformation("[BEAT] removing beat: {0}:{1}:{2} from beat map.", serviceName, ip, port);
            string key = BuildKey(serviceName, ip, port);

            if (_dom2Beat.TryRemove(key, out var beatInfo)) beatInfo.Stopped = true;

            if (_beatTimer.TryRemove(key, out var t)) t.Dispose();
        }
    }
}
