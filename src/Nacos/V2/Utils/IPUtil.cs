namespace Nacos.V2.Utils
{
    using System;
    using System.Text;

    public class IPUtil
    {
        public static bool PREFER_IPV6_ADDRESSES = Boolean.Parse(Environment.GetEnvironmentVariable("java.net.preferIPv6Addresses"));

        public static string IPV6_START_MARK = "[";

        public static string IPV6_END_MARK = "]";

        public static string ILLEGAL_IP_PREFIX = "illegal ip: ";

        public static string IP_PORT_SPLITER = ":";

        public static int SPLIT_IP_PORT_RESULT_LENGTH = 2;

        public static string PERCENT_SIGN_IN_IPV6 = "%";

        private static readonly string LOCAL_HOST_IP_V4 = "127.0.0.1";

        private static readonly string LOCAL_HOST_IP_V6 = "[::1]";

        /*private static Pattern ipv4Pattern = Pattern.compile("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}");

        private static readonly int IPV4_ADDRESS_LENGTH = 4;

        private static readonly int IPV6_ADDRESS_LENGTH = 16;*/

        private static readonly string CHECK_OK = "ok";

        public static string LocalHostIP()
        {
            if (PREFER_IPV6_ADDRESSES)
            {
                return LOCAL_HOST_IP_V6;
            }

            return LOCAL_HOST_IP_V4;
        }


        public static bool IsIPv4(string addr)
        {
            return System.Net.IPAddress.TryParse(addr, out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }


        public static bool IsIPv6(string addr)
        {
            return System.Net.IPAddress.TryParse(addr, out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
        }

        public static bool IsIP(string addr)
        {
            return System.Net.IPAddress.TryParse(addr, out _);
        }

        public static bool ContainsPort(string address)
        {
            return SplitIPPortStr(address).Length == SPLIT_IP_PORT_RESULT_LENGTH;
        }

        public static string[] SplitIPPortStr(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException("ip and port string cannot be empty!");

            string[] serverAddrArr;
            if (str.StartsWith(IPV6_START_MARK) && str.Contains(IPV6_END_MARK))
            {
                if (str.EndsWith(IPV6_END_MARK))
                {
                    serverAddrArr = new string[1];
                    serverAddrArr[0] = str;
                }
                else
                {
                    serverAddrArr = new string[2];
                    serverAddrArr[0] = str.Substring(0, str.IndexOf(IPV6_END_MARK) + 1);
                    serverAddrArr[1] = str.Substring(str.IndexOf(IPV6_END_MARK) + 2);
                }

                if (!IsIPv6(serverAddrArr[0]))
                {
                    throw new ArgumentException("The IPv6 address(\"" + serverAddrArr[0] + "\") is incorrect.");
                }
            }
            else
            {
                serverAddrArr = str.Split(':');
                if (serverAddrArr.Length > SPLIT_IP_PORT_RESULT_LENGTH)
                {
                    throw new ArgumentException("The IP address(\"" + str
                            + "\") is incorrect. If it is an IPv6 address, please use [] to enclose the IP part!");
                }

                if (!IsIPv4(serverAddrArr[0]))
                {
                    throw new ArgumentException("The IPv4 address(\"" + serverAddrArr[0] + "\") is incorrect.");
                }
            }

            return serverAddrArr;
        }

        public static string CheckIPs(params string[] ips)
        {
            if (ips == null || ips.Length == 0) return CHECK_OK;

            // illegal response
            StringBuilder illegalResponse = new StringBuilder();
            foreach (string ip in ips)
            {
                if (IsIP(ip)) continue;

                illegalResponse.Append(ip + ",");
            }

            if (illegalResponse.Length == 0) return CHECK_OK;

            var resp = illegalResponse.ToString();

            return ILLEGAL_IP_PREFIX + resp.Substring(0, resp.Length - 1);
        }

        public static bool CheckOK(string checkIPsResult)
        {
            return CHECK_OK.Equals(checkIPsResult);
        }
    }
}
