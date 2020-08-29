namespace Nacos.AspNetCore
{
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class StatusReportBgTask : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly INacosNamingClient _client;
        private readonly IFeatureCollection _features;
        private NacosAspNetCoreOptions _options;

        private Timer _timer;
        private bool _reporting;
        private Uri uri = null;
        private SendHeartbeatRequest beatRequest = null;

        public StatusReportBgTask(
            ILoggerFactory loggerFactory,
            INacosNamingClient client,
            IServer server,
            IOptionsMonitor<NacosAspNetCoreOptions> optionsAccs)
        {
            _logger = loggerFactory.CreateLogger<StatusReportBgTask>();
            _client = client;
            _options = optionsAccs.CurrentValue;
            _features = server.Features;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            uri = UriTool.GetUri(_features, _options);

            _logger.LogInformation("Report instance ({0}:{1}) status....", uri.Host, uri.Port);

            var metadata = new Dictionary<string, string>()
            {
                { PreservedMetadataKeys.REGISTER_SOURCE, "ASPNET_CORE" }
            };

            foreach (var item in _options.Metadata)
            {
                if (!metadata.ContainsKey(item.Key))
                {
                    metadata.TryAdd(item.Key, item.Value);
                }
            }

            beatRequest = new SendHeartbeatRequest
            {
                Ephemeral = true,
                ServiceName = _options.ServiceName,
                GroupName = _options.GroupName,
                BeatInfo = new BeatInfo
                {
                    ip = uri.Host,
                    port = uri.Port,
                    serviceName = _options.ServiceName,
                    scheduled = true,
                    weight = _options.Weight,
                    cluster = _options.ClusterName,
                    metadata = metadata,
                },
                NameSpaceId = _options.Namespace
            };

            _timer = new Timer(
                async x =>
                {
                    if (_reporting)
                    {
                        _logger.LogInformation($"Latest manipulation is still working ...");
                        return;
                    }

                    _reporting = true;
                    await ReportAsync();
                    _reporting = false;
                }, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private async Task ReportAsync()
        {
            bool flag = false;

            try
            {
                // send heart beat will register instance
                flag = await _client.SendHeartbeatAsync(beatRequest);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Send heart beat to Nacos error");
            }

            _logger.LogDebug("report at {0}, status = {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), flag);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Unregistering from Nacos, serviceName={0}", _options.ServiceName);

            var removeRequest = new RemoveInstanceRequest
            {
                ServiceName = _options.ServiceName,
                Ip = uri.Host,
                Port = uri.Port,
                GroupName = _options.GroupName,
                NamespaceId = _options.Namespace,
                ClusterName = _options.ClusterName,
                Ephemeral = true
            };

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _logger.LogWarning("begin to remove instance, {0}", JsonConvert.SerializeObject(removeRequest));
                    var flag = await _client.RemoveInstanceAsync(removeRequest);
                    _logger.LogWarning("remove instance, status = {0}", flag);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unregistering error, count = {0}", i + 1);
                }
            }

            _timer?.Change(Timeout.Infinite, 0);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
