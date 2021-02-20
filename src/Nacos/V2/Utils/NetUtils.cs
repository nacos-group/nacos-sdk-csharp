namespace Nacos.V2.Utils
{
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    public class NetUtils
    {
        private static readonly string ResolveFailed = "resolve_failed";

        private static string localIp;

        public static string LocalIP()
        {
            try
            {
                if (localIp.IsNotNullOrWhiteSpace()) return localIp;

                var instanceIp = string.Empty;

                // 获取可用网卡
                var nics = NetworkInterface.GetAllNetworkInterfaces()?.Where(network => network.OperationalStatus == OperationalStatus.Up);

                // 获取所有可用网卡IP信息
                var ipCollection = nics?.Select(x => x.GetIPProperties())?.SelectMany(x => x.UnicastAddresses);

                foreach (var ipadd in ipCollection)
                {
                    if (!IPAddress.IsLoopback(ipadd.Address) && ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        instanceIp = ipadd.Address.ToString();
                        break;
                    }
                }

                return instanceIp.IsNotNullOrWhiteSpace() ? localIp = instanceIp : ResolveFailed;
            }
            catch
            {
                return ResolveFailed;
            }
        }
    }
}
