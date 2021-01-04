namespace Nacos.V2.Naming.Beat
{
    using Nacos.V2.Common;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class BeatReactor
    {
        private Nacos.V2.Naming.Remote.Http.NamingHttpClientProxy _serverProxy;

        public BeatReactor(Nacos.V2.Naming.Remote.Http.NamingHttpClientProxy serverProxy, NacosSdkOptions options)
        {
            this._serverProxy = serverProxy;
        }

        public ConcurrentDictionary<string, BeatInfo> Dom2Beat = new ConcurrentDictionary<string, BeatInfo>();

        private readonly ConcurrentDictionary<string, Timer> _beatTimer = new ConcurrentDictionary<string, Timer>();

        internal string BuildKey(string serviceName, string ip, int port)
        {
            return serviceName + Constants.NAMING_INSTANCE_ID_SPLITTER + ip + Constants.NAMING_INSTANCE_ID_SPLITTER + port;
        }

        internal BeatInfo BuildBeatInfo(Instance instance)
        {
            return BuildBeatInfo(instance.ServiceName, instance);
        }

        internal void AddBeatInfo(string serviceName, BeatInfo beatInfo)
        {
            // TODO logger
            string key = BuildKey(serviceName, beatInfo.ip, beatInfo.port);

            if (Dom2Beat.TryRemove(key, out var exitBeat)) exitBeat.stopped = true;

            Dom2Beat[key] = beatInfo;

            var timer = new Timer(
                async x =>
                {
                    var info = x as BeatInfo;
                    await BeatTask(info);
                }, beatInfo, beatInfo.period, beatInfo.period);

            _beatTimer[key] = timer;
        }

        private async Task BeatTask(BeatInfo beatInfo)
        {
            if (beatInfo.stopped) return;

            long nextTime = beatInfo.period;

            try
            {
                Newtonsoft.Json.Linq.JObject result = await _serverProxy.SendBeat(beatInfo, false);

                long interval = result.GetValue("clientBeatInterval").ToObject<long>();

                bool lightBeatEnabled = false;

                if (result.ContainsKey(CommonParams.LIGHT_BEAT_ENABLED))
                {
                    lightBeatEnabled = result.GetValue(CommonParams.LIGHT_BEAT_ENABLED).ToObject<bool>();
                }

                if (interval > 0) nextTime = interval;

                int code = 10200;

                if (result.ContainsKey(CommonParams.CODE)) code = result.GetValue(CommonParams.CODE).ToObject<int>();

                if (code == 20404)
                {
                    Instance instance = new Instance
                    {
                        Port = beatInfo.port,
                        Ip = beatInfo.ip,
                        Weight = beatInfo.weight ?? 1,
                        Metadata = beatInfo.metadata,
                        ClusterName = beatInfo.cluster,
                        ServiceName = beatInfo.serviceName,
                        Ephemeral = true,

                        // InstanceId = ""
                    };

                    try
                    {
                        await _serverProxy.RegisterServiceAsync(beatInfo.serviceName, NamingUtils.GetGroupName(beatInfo.serviceName), instance);
                    }
                    catch
                    {
                    }
                }
            }
            catch (NacosException ex)
            {
                Console.WriteLine(ex);
            }

            var timer = new Timer(
                async x =>
                {
                    var info = x as BeatInfo;
                    await BeatTask(info);
                }, beatInfo, nextTime, nextTime);


            string key = BuildKey(beatInfo.serviceName, beatInfo.ip, beatInfo.port);
            _beatTimer[key] = timer;
        }

        internal BeatInfo BuildBeatInfo(string groupedServiceName, Instance instance)
        {
            return new BeatInfo
            {
                serviceName = groupedServiceName,
                ip = instance.Ip,
                port = instance.Port,
                cluster = instance.ClusterName,
                weight = instance.Weight,
                metadata = instance.Metadata,
                period = instance.GetInstanceHeartBeatInterval(),
                scheduled = false
            };
        }

        internal void RemoveBeatInfo(string serviceName, string ip, int port)
        {
            // TODO logger
            string key = BuildKey(serviceName, ip, port);

            if (Dom2Beat.TryRemove(key, out var beatInfo)) beatInfo.stopped = true;

            if (_beatTimer.TryRemove(key, out var t)) t.Dispose();
        }
    }
}
