﻿namespace Nacos.Naming.Backups
{
    using Microsoft.Extensions.Logging;
    using Nacos.Logging;
    using Nacos.Naming.Cache;
    using Nacos.Naming.Utils;
    using Nacos.Utils;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class FailoverReactor : IDisposable
    {
        private static readonly string FAILOVER_DIR = "failover";

        private static readonly string IS_FAILOVER_MODE = "1";

        private static readonly string NO_FAILOVER_MODE = "0";

        private static readonly string FAILOVER_MODE_PARAM = "failover-mode";


/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
        private ConcurrentDictionary<string, V2.Naming.Dtos.ServiceInfo> serviceMap = new ConcurrentDictionary<string, V2.Naming.Dtos.ServiceInfo>();
在此之后:
        private ConcurrentDictionary<string, ServiceInfo> serviceMap = new ConcurrentDictionary<string, ServiceInfo>();
*/
        private ConcurrentDictionary<string, Dtos.ServiceInfo> serviceMap = new ConcurrentDictionary<string, Dtos.ServiceInfo>();

        private ConcurrentDictionary<string, string> switchParams = new ConcurrentDictionary<string, string>();

        private static readonly long DAY_PERIOD_MINUTES = 24 * 60;

        private readonly ILogger _logger = NacosLogManager.CreateLogger<FailoverReactor>();
        private readonly string _failoverDir;
        private readonly ServiceInfoHolder _serviceInfoHolder;

        private readonly Timer _switchRefresherTimer;
        private readonly Timer _diskFileWriterTimer;
        private readonly Timer _timer;

        private long lastModifiedMillis = 0L;

        public FailoverReactor(ServiceInfoHolder serviceInfoHolder, string cacheDir)
        {
            _serviceInfoHolder = serviceInfoHolder;
            _failoverDir = Path.Combine(cacheDir, FAILOVER_DIR);

            _switchRefresherTimer = new Timer(
                async x => await RunSwitchRefresh().ConfigureAwait(false), null, 0, 5000);

            _diskFileWriterTimer = new Timer(
                async x => await RunDiskFileWrite().ConfigureAwait(false), null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(DAY_PERIOD_MINUTES));

            _timer = new Timer(
                async x => await RunUpdateBackupFile().ConfigureAwait(false), null, TimeSpan.FromMilliseconds(10000), Timeout.InfiniteTimeSpan);
        }

        public async Task RunUpdateBackupFile()
        {
            try
            {
                var cacheDir = new DirectoryInfo(_failoverDir);
                if (!cacheDir.Exists)
                {
                    try
                    {
                        cacheDir.Create();
                    }
                    catch (Exception)
                    {
                        throw new Exception($"failed to create cache dir: {_failoverDir}");
                    }
                }

                var files = cacheDir.GetFiles();
                if (files == null || files.Length <= 0)
                {
                    await RunDiskFileWrite().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[NA] failed to backup file on startup.");
            }
        }

        public async Task RunSwitchRefresh()
        {
            try
            {
                var switchFile = new FileInfo(Path.Combine(_failoverDir, UtilAndComs.FAILOVER_SWITCH));
                if (!switchFile.Exists)
                {
                    switchParams.AddOrUpdate(FAILOVER_MODE_PARAM, bool.FalseString, (x, y) => bool.FalseString);
                    _logger?.LogDebug("failover switch is not found, {0}", switchFile.Name);
                    return;
                }

                long modified = switchFile.LastWriteTime.ToTimestamp();

                if (lastModifiedMillis < modified)
                {
                    lastModifiedMillis = modified;

                    string failover = await switchFile.ReadFileAsync().ConfigureAwait(false);

                    if (failover.IsNotNullOrWhiteSpace())
                    {
                        string[] lines = failover.SplitByString(DiskCache.GetLineSeparator());

                        foreach (var line in lines)
                        {
                            string l = line.Trim();

                            if (IS_FAILOVER_MODE.Equals(l))
                            {
                                switchParams.AddOrUpdate(FAILOVER_MODE_PARAM, bool.TrueString, (x, y) => bool.TrueString);

                                _logger?.LogInformation("failover-mode is on");

                                await RunFailoverFileRead().ConfigureAwait(false);
                            }
                            else if (NO_FAILOVER_MODE.Equals(l))
                            {
                                switchParams.AddOrUpdate(FAILOVER_MODE_PARAM, bool.FalseString, (x, y) => bool.FalseString);

                                _logger?.LogInformation("failover-mode is off");
                            }
                        }
                    }
                    else
                    {
                        switchParams.AddOrUpdate(FAILOVER_MODE_PARAM, bool.FalseString, (x, y) => bool.FalseString);
                    }
                }
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "[NA] failed to read failover switch.");
            }
        }

        public async Task RunFailoverFileRead()
        {
/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
            var domMap = new ConcurrentDictionary<string, V2.Naming.Dtos.ServiceInfo>();
在此之后:
            var domMap = new ConcurrentDictionary<string, ServiceInfo>();
*/
            var domMap = new ConcurrentDictionary<string, Dtos.ServiceInfo>();

            try
            {
                var cacheDir = new DirectoryInfo(_failoverDir);
                if (!cacheDir.Exists)
                {
                    try
                    {
                        cacheDir.Create();
                    }
                    catch (Exception)
                    {
                        throw new Exception($"failed to create cache dir: {_failoverDir}");
                    }
                }

                var files = cacheDir.GetFiles();
                if (files == null) return;

                foreach (var file in files)
                {
                    if (file.Name.Equals(UtilAndComs.FAILOVER_SWITCH)) continue;

                    var dom = new Naming.Dtos.ServiceInfo(file.Name);

                    try
                    {
                        var dataString = await file.ReadFileAsync().ConfigureAwait(false);

                        using (StringReader reader = new StringReader(dataString))
                        {
                            var json = reader.ReadLine();

                            if (json.IsNotNullOrWhiteSpace())
                            {
                                try
                                {
/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
                                    dom = json.ToObj<V2.Naming.Dtos.ServiceInfo>();
在此之后:
                                    dom = json.ToObj<ServiceInfo>();
*/
                                    dom = json.ToObj<Dtos.ServiceInfo>();
                                }
                                catch (Exception e)
                                {
                                    _logger?.LogError(e, "[NA] error while parsing cached dom : {0}", json);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignore
                    }

                    if (dom.Hosts != null && dom.Hosts.Any())
                    {
                        domMap[dom.GetKey()] = dom;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[NA] failed to read cache file");
            }

            if (domMap != null && domMap.Any())
            {
                serviceMap = domMap;
            }
        }

        public async Task RunDiskFileWrite()
        {
            var map = _serviceInfoHolder.GetServiceInfoMap();
            foreach (var entry in map)
            {
                var serviceInfo = entry.Value;

                if (serviceInfo.GetKey().Equals(UtilAndComs.ALL_IPS) ||
                    serviceInfo.Name.Equals(UtilAndComs.ENV_LIST_KEY) ||
                    serviceInfo.Name.Equals(UtilAndComs.ENV_CONFIGS) ||
                    serviceInfo.Name.Equals(UtilAndComs.VIP_CLIENT_FILE) ||
                    serviceInfo.Name.Equals(UtilAndComs.ALL_HOSTS))
                {
                    continue;
                }

                await DiskCache.WriteAsync(serviceInfo, _failoverDir).ConfigureAwait(false);
            }
        }

        public bool IsFailoverSwitch()
        {
            return switchParams.TryGetValue(FAILOVER_MODE_PARAM, out var flag)
                ? bool.Parse(flag)
                : false;
        }


/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
        public V2.Naming.Dtos.ServiceInfo GetService(string key)
在此之后:
        public ServiceInfo GetService(string key)
*/
        public Dtos.ServiceInfo GetService(string key)
        {
            if (!serviceMap.TryGetValue(key, out var serviceInfo))
            {
                serviceInfo = new Naming.Dtos.ServiceInfo();
                serviceInfo.Name = key;
            }

            return serviceInfo;
        }

        public void Dispose()
        {
            _logger?.LogInformation("{0} do shutdown begin", nameof(FailoverReactor));
            _timer?.Dispose();
            _diskFileWriterTimer?.Dispose();
            _switchRefresherTimer?.Dispose();
            _logger?.LogInformation("{0} do shutdown stop", nameof(FailoverReactor));
        }
    }
}
