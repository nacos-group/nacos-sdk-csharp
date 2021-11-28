namespace Nacos.V2.Utils
{
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    public class NetUtils
    {
        private static readonly string ResolveFailed = "resolve_failed";
        private static readonly string CLIENT_NAMING_LOCAL_IP_PROPERTY = "com.alibaba.nacos.client.naming.local.ip";
        private static readonly string CLIENT_LOCAL_IP_PROPERTY = "com.alibaba.nacos.client.local.ip";
        private static string localIp;

        public static string LocalIP()
        {
            if (localIp.IsNotNullOrWhiteSpace()) return localIp;

            var val = EnvUtil.GetEnvValue(CLIENT_LOCAL_IP_PROPERTY);

            if (val.IsNotNullOrWhiteSpace()) return val;

            val = EnvUtil.GetEnvValue(CLIENT_NAMING_LOCAL_IP_PROPERTY);

            var ip = val.IsNullOrWhiteSpace() ? FindFirstNonLoopbackAddress() : val;

            return localIp = ip;
        }

        private static string FindFirstNonLoopbackAddress()
        {
            var instanceIp = string.Empty;

            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces()?.Where(network => network.OperationalStatus == OperationalStatus.Up);

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
                // ignore
            }

            return ResolveFailed;
        }
    }
}
