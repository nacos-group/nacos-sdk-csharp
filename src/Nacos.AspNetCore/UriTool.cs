﻿using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Nacos.AspNetCore.Tests")]

namespace Nacos.AspNetCore
{
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    internal static class UriTool
    {
        public static IEnumerable<Uri> GetUri(IFeatureCollection features, string ip, int port, string preferredNetworks)
        {
            var splitChars = new char[] { ',', ';' };
            var appPort = port <= 0 ? 80 : port;

            // 1. config
            if (!string.IsNullOrWhiteSpace(ip))
            {
                // it seems that nacos don't return the scheme
                // so here use http only.
                return new List<Uri> { new Uri($"http://{ip}:{appPort}") };
            }

            // 1.1. Ip is null && Port has value
            if (string.IsNullOrWhiteSpace(ip) && appPort != 80)
            {
                return new List<Uri> { new Uri($"http://{GetCurrentIp(preferredNetworks)}:{appPort}") };
            }

            var address = string.Empty;

            // 2. IServerAddressesFeature
            if (features != null)
            {
                var addresses = features.Get<IServerAddressesFeature>();
                var addressCollection = addresses?.Addresses;

                if (addressCollection != null && addressCollection.Any())
                {
                    var uris = new List<Uri>();
                    foreach (var item in addressCollection)
                    {
                        var url = ReplaceAddress(item, preferredNetworks);
                        uris.Add(new Uri(url));
                    }

                    return uris;
                }
            }

            // 3. ASPNETCORE_URLS
            address = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (!string.IsNullOrWhiteSpace(address))
            {
                var url = ReplaceAddress(address, preferredNetworks);

                var uris = url.Split(splitChars).Select(x => new Uri(x));

                foreach (var item in uris)
                {
                    if (!IPAddress.TryParse(item.Host, out _))
                    {
                        throw new Nacos.V2.Exceptions.NacosException("Invalid ip address from ASPNETCORE_URLS");
                    }
                }

                return uris;
            }

            // 4. --urls
            var cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs != null && cmdArgs.Any())
            {
                var cmd = cmdArgs.FirstOrDefault(x => x.StartsWith("--urls", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(cmd))
                {
                    address = cmd.Split('=')[1];

                    var url = ReplaceAddress(address, preferredNetworks);

                    var uris = url.Split(splitChars).Select(x => new Uri(x));

                    foreach (var item in uris)
                    {
                        if (!IPAddress.TryParse(item.Host, out _))
                        {
                            throw new Nacos.V2.Exceptions.NacosException("Invalid ip address from --urls");
                        }
                    }

                    return uris;
                }
            }

            // 5. current ip address third
            address = $"http://{GetCurrentIp(preferredNetworks)}:{appPort}";

            return new List<Uri> { new Uri(address) };
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
                // 获取可用网卡
                var nics = NetworkInterface.GetAllNetworkInterfaces()?.Where(network => network.OperationalStatus == OperationalStatus.Up);

                // 获取所有可用网卡IP信息
                var ipCollection = nics?.Select(x => x.GetIPProperties())?.SelectMany(x => x.UnicastAddresses);

                var preferredNetworksArr = preferredNetworks.Split(",");
                foreach (var ipadd in ipCollection)
                {
                    if (!IPAddress.IsLoopback(ipadd.Address) &&
                        ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (string.IsNullOrEmpty(preferredNetworks))
                        {
                            instanceIp = ipadd.Address.ToString();
                            break;
                        }

                        if (!preferredNetworksArr.Any(preferredNetwork =>
                                ipadd.Address.ToString().StartsWith(preferredNetwork)
                                || Regex.IsMatch(ipadd.Address.ToString(), preferredNetwork))) continue;
                        instanceIp = ipadd.Address.ToString();
                        break;
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
