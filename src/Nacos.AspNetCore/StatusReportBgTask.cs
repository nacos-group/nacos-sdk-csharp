namespace Nacos.AspNetCore
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class StatusReportBgTask : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly INacosNamingClient _client;
        private readonly IFeatureCollection _features;
        private NacosAspNetCoreOptions _options;

        private Timer _timer;
        private bool _reporting;
        private Uri uri = null;

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
            uri = GetUri(_features, _options);

            _logger.LogInformation("Report instance ({0}:{1}) status....", uri.Host, uri.Port);

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
            Console.WriteLine(Task.CompletedTask);
            return Task.CompletedTask;
        }

        private async Task ReportAsync()
        {
            bool flag = false;

            try
            {
                // send heart beat will register instance
                Console.WriteLine("Sending Heartbeat");
                flag = await _client.SendHeartbeatAsync(new SendHeartbeatRequest
                {
                    Ephemeral = false,
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
                        metadata = _options.Metadata,
                    },
                    NameSpaceId = _options.Namespace
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Send heart beat to Nacos error");
            }

            _logger.LogDebug("report at {0}, status = {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), flag);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Unregistering from Nacos");

            var removeRequest = new RemoveInstanceRequest
            {
                ServiceName = _options.ServiceName,
                Ip = uri.Host,
                Port = uri.Port,
                GroupName = _options.GroupName,
                NamespaceId = _options.Namespace,
                ClusterName = _options.ClusterName,
                Ephemeral = false
            };

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var flag = await _client.RemoveInstanceAsync(removeRequest);
                    _logger.LogDebug("remove instance, status = {0}", flag);
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

        private Uri GetUri(IFeatureCollection features, NacosAspNetCoreOptions config)
        {
            var port = config.Port <= 0 ? 80 : config.Port;

            // 1. config
            if (!string.IsNullOrWhiteSpace(config.Ip))
            {
                // it seems that nacos don't return the scheme
                // so here use http only.
                return new Uri($"http://{config.Ip}:{port}");
            }

            var address = string.Empty;

            // 2. IServerAddressesFeature
            if (features != null)
            {
                var addresses = features.Get<IServerAddressesFeature>();
                address = addresses?.Addresses?.FirstOrDefault();

                if (address != null)
                {
                    var url = ReplaceAddress(address);
                    return new Uri(url);
                }
            }

            // 3. ASPNETCORE_URLS
            address = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (!string.IsNullOrWhiteSpace(address))
            {
                var url = ReplaceAddress(address);
                return new Uri(url);
            }

            // 4. --urls
            var cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs != null && cmdArgs.Any())
            {
                var cmd = cmdArgs.FirstOrDefault(x => x.StartsWith("--urls", StringComparison.OrdinalIgnoreCase));
                address = cmd.Split('=')[1];

                var url = ReplaceAddress(address);
                return new Uri(url);
            }

            // 5. current ip address third
            address = $"http://{GetCurrentIp()}:{port}";

            return new Uri(address);
        }

        private string ReplaceAddress(string address)
        {
            var ip = GetCurrentIp();

            if (address.Contains("*"))
            {
                address = address.Replace("*", ip);
            }
            else if (address.Contains("+"))
            {
                address = address.Replace("+", ip);
            }
            else if (address.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                address = address.Replace("localhost", ip, StringComparison.OrdinalIgnoreCase);
            }
            else if (address.Contains("0.0.0.0", StringComparison.OrdinalIgnoreCase))
            {
                address = address.Replace("0.0.0.0", ip, StringComparison.OrdinalIgnoreCase);
            }

            return address;
        }

        private string GetCurrentIp()
        {
            var instanceIp = "127.0.0.1";

            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());

                foreach (var ipAddr in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (ipAddr.AddressFamily.ToString() == "InterNetwork")
                    {
                        instanceIp = ipAddr.ToString();
                        break;
                    }
                }
            }
            catch
            {
            }

            return instanceIp;
        }
    }
}
