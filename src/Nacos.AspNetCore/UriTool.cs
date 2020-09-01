namespace Nacos.AspNetCore
{
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    internal static class UriTool
    {
        public static Uri GetUri(IFeatureCollection features, NacosAspNetCoreOptions config)
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
                    var url = ReplaceAddress(address, config.PreferredNetworks);
                    return new Uri(url);
                }
            }

            // 3. ASPNETCORE_URLS
            address = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (!string.IsNullOrWhiteSpace(address))
            {
                var url = ReplaceAddress(address, config.PreferredNetworks);
                return new Uri(url);
            }

            // 4. --urls
            var cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs != null && cmdArgs.Any())
            {
                var cmd = cmdArgs.FirstOrDefault(x => x.StartsWith("--urls", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(cmd))
                {
                    address = cmd.Split('=')[1];

                    var url = ReplaceAddress(address, config.PreferredNetworks);
                    return new Uri(url);
                }
            }

            // 5. current ip address third
            address = $"http://{GetCurrentIp(config.PreferredNetworks)}:{port}";

            return new Uri(address);
        }

        private static string ReplaceAddress(string address, string preferredNetworks)
        {
            var ip = GetCurrentIp(preferredNetworks);

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

        private static string GetCurrentIp(string preferredNetworks)
        {
            var instanceIp = "127.0.0.1";

            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces().Where(network => network.OperationalStatus == OperationalStatus.Up);

                foreach (var nic in nics)
                {
                    var ip = nic.GetIPProperties();
                    var ipCollection = ip.UnicastAddresses;
                    foreach (var ipadd in ipCollection)
                    {
                        if (!IPAddress.IsLoopback(ipadd.Address) && ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (string.IsNullOrEmpty(preferredNetworks))
                            {
                                instanceIp = ipadd.Address.ToString();
                                break;
                            }

                            if (!ipadd.ToString().StartsWith(preferredNetworks)) continue;
                            instanceIp = ipadd.ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            return instanceIp;
        }
    }
}
