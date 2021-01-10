namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Nacos.Utilities;
    using Nacos.V2.Common;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class FailoverReactor : IDisposable
    {
        public static string FAILOVER_MODE_NAME = "failover-mode";
        public static string FAILOVER_PATH = "failover";

        private readonly ILogger _logger;

        private string _failoverDir;
        private HostReactor _hostReactor;
        private DiskCache _diskCache;

        private Timer _switchRefresher;
        private Timer _diskFileWriter;

        private ConcurrentDictionary<string, ServiceInfo> _serviceMap = new ConcurrentDictionary<string, ServiceInfo>();
        private ConcurrentDictionary<string, string> _switchParams = new ConcurrentDictionary<string, string>();

        public FailoverReactor(ILoggerFactory loggerFactory, HostReactor hostReactor, DiskCache diskCache, string cacheDir)
        {
            _logger = loggerFactory.CreateLogger<FailoverReactor>();
            _hostReactor = hostReactor;
            _diskCache = diskCache;
            _failoverDir = Path.Combine(cacheDir, FAILOVER_PATH);

            Init();
        }

        public void Init()
        {
            SwitchRefresher();
            DiskFileWriter();
        }

        private void SwitchRefresher()
        {
            long lastModifiedMillis = 0;

            _switchRefresher = new Timer(
              async x =>
              {
                  try
                  {
                      string filePath = Path.Combine(_failoverDir, ConstValue.FAILOVER_SWITCH);

                      if (!File.Exists(filePath))
                      {
                          _switchParams.AddOrUpdate(FAILOVER_MODE_NAME, "false", (k, v) => "false");
                          _logger.LogDebug("failover switch is not found, {0}", filePath);
                          return;
                      }

                      long modified = _diskCache.GetFileLastModifiedTime(filePath);

                      if (lastModifiedMillis < modified)
                      {
                          lastModifiedMillis = modified;
                          string failover = await _diskCache.ReadFile(filePath);

                          if (!string.IsNullOrEmpty(failover))
                          {
                              var lines = failover.SplitByString(_diskCache.GetLineSeparator());
                              foreach (var line in lines)
                              {
                                  if ("1".Equals(line.Trim()))
                                  {
                                      _switchParams.AddOrUpdate(FAILOVER_MODE_NAME, "true", (k, v) => "true");
                                      _logger.LogInformation($"{FAILOVER_MODE_NAME} is on");
                                      await FailoverFileReader();
                                  }
                                  else if ("0".Equals(line.Trim()))
                                  {
                                      _switchParams.AddOrUpdate(FAILOVER_MODE_NAME, "false", (k, v) => "false");
                                      _logger.LogInformation($"{FAILOVER_MODE_NAME} is off");
                                  }
                              }
                          }
                          else
                          {
                              _switchParams.AddOrUpdate(FAILOVER_MODE_NAME, "false", (k, v) => "false");
                          }
                      }
                  }
                  catch (Exception ex)
                  {
                      _logger.LogError(ex, "[NA] failed to read failover switch.");
                  }
              }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(5000));
        }

        private void DiskFileWriter()
        {
            _diskFileWriter = new Timer(
                async x =>
                {
                    var map = _hostReactor.GetServiceInfoMap();
                    foreach (var entry in map)
                    {
                        ServiceInfo serviceInfo = entry.Value;

                        if (serviceInfo.GetKey().Equals(ConstValue.ALL_IPS)
                         || serviceInfo.name.Equals(ConstValue.ENV_LIST_KEY)
                         || serviceInfo.name.Equals(ConstValue.ENV_CONFIGS)
                         || serviceInfo.name.Equals(ConstValue.VIPCLIENT_CONFIG)
                         || serviceInfo.name.Equals(ConstValue.ALL_HOSTS))
                        {
                            continue;
                        }

                        await _diskCache.WriteServiceInfoAsync(_failoverDir, serviceInfo);
                    }
                }, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(24 * 60));
        }

        private async Task FailoverFileReader()
        {
            var domMap = new ConcurrentDictionary<string, ServiceInfo>();
            try
            {
                var files = _diskCache.MakeSureCacheDirExists(_failoverDir);

                foreach (var filePath in files)
                {
                    var fi = new FileInfo(filePath);
                    if (fi.Name.Equals(ConstValue.FAILOVER_SWITCH)) continue;

                    string content = await _diskCache.ReadFile(filePath);
                    ServiceInfo serviceInfo = content.ToObj<ServiceInfo>();
                    if (serviceInfo.Hosts != null && serviceInfo.Hosts.Count > 0)
                    {
                        domMap.AddOrUpdate(serviceInfo.GetKey(), serviceInfo, (k, v) => serviceInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NA] failed to read cache files");
            }

            if (domMap.Count > 0)
            {
                _serviceMap = domMap;
            }
        }

        public bool IsFailoverSwitch()
        {
            if (_switchParams.TryGetValue(FAILOVER_MODE_NAME, out string failover))
            {
                return !failover.Equals("false");
            }

            return false;
        }

        public ServiceInfo GetService(string key)
        {
            if (!_serviceMap.TryGetValue(key, out ServiceInfo serviceInfo))
            {
                serviceInfo = new ServiceInfo
                {
                    name = key
                };
            }

            return serviceInfo;
        }

        public void Dispose()
        {
            _diskFileWriter?.Dispose();
            _switchRefresher?.Dispose();
        }
    }
}