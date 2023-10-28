namespace Nacos.Tests.Utils
{
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class IPUtilTest
    {
        [Fact]
        public void LocalHostIP_Should_Succeed()
        {
            var ip = IPUtil.LocalHostIP();
            Assert.Equal("127.0.0.1", ip);

            Environment.SetEnvironmentVariable("java.net.preferIPv6Addresses", bool.TrueString);
            ip = IPUtil.LocalHostIP();
            Assert.Equal("[::1]", ip);

            Environment.SetEnvironmentVariable("java.net.preferIPv6Addresses", bool.FalseString);
            IPUtil.PREFER_IPV6_ADDRESSES = bool.Parse(Environment.GetEnvironmentVariable("java.net.preferIPv6Addresses"));
            ip = IPUtil.LocalHostIP();
            Assert.Equal("127.0.0.1", ip);
        }

        [Fact]
        public void IsIPv4_Should_Succeed()
        {
            Assert.True(IPUtil.IsIPv4("192.168.1.1"));

            Assert.False(IPUtil.IsIPv4("192.168.1.1.3"));

            Assert.False(IPUtil.IsIPv4("[::1]"));
        }

        [Fact]
        public void IsIPv6_Should_Succeed()
        {
            Assert.False(IPUtil.IsIPv6("192.168.1.1"));

            Assert.False(IPUtil.IsIPv6("192.168.1.1.3"));

            Assert.True(IPUtil.IsIPv6("[::1]"));
        }

        [Fact]
        public void IsIP_Should_Succeed()
        {
            Assert.True(IPUtil.IsIP("192.168.1.1"));

            Assert.True(IPUtil.IsIP("[::1]"));

            Assert.False(IPUtil.IsIP("192.168.1.1.3"));

            Assert.False(IPUtil.IsIP("[::1w-3]"));
        }

        [Fact]
        public void SplitIPPortStr_Should_Succeed()
        {
            var ipv4 = "192.168.1.1:80";
            var ipv6 = "[A01F::0]:88";

            var ipv4Port = IPUtil.SplitIPPortStr(ipv4);
            Assert.Equal(2, ipv4Port.Length);
            Assert.Equal("192.168.1.1", ipv4Port[0]);
            Assert.Equal("80", ipv4Port[1]);

            var ipv6Port = IPUtil.SplitIPPortStr(ipv6);
            Assert.Equal(2, ipv6Port.Length);
            Assert.Equal("[A01F::0]", ipv6Port[0]);
            Assert.Equal("88", ipv6Port[1]);
        }

        [Fact]
        public void ContainsPort_Should_Succeed()
        {
            // regular address
            var ipv4 = "192.168.1.1:80";
            var ipv6 = "[A01F::0]:88";
            var ipv4WithPort = IPUtil.ContainsPort(ipv4);
            Assert.True(ipv4WithPort);

            var ipv6WithPort = IPUtil.ContainsPort(ipv6);
            Assert.True(ipv6WithPort);

            // with null port
            ipv4 = "192.168.1.1:";
            ipv6 = "[A01F::0]:";
            ipv4WithPort = IPUtil.ContainsPort(ipv4);
            Assert.False(ipv4WithPort);

            ipv6WithPort = IPUtil.ContainsPort(ipv6);
            Assert.False(ipv6WithPort);

            // without port
            ipv4 = "192.168.1.1";
            ipv6 = "[A01F::0]";
            ipv4WithPort = IPUtil.ContainsPort(ipv4);
            Assert.False(ipv4WithPort);

            ipv6WithPort = IPUtil.ContainsPort(ipv6);
            Assert.False(ipv6WithPort);
        }

        [Fact]
        public void CheckIPs_Should_Succeed()
        {
            var ips = new List<string>();
            var illegalIps = IPUtil.CheckIPs(ips.ToArray());
            Assert.Equal("ok", illegalIps);

            ips = new List<string> { "[::1]", "[::1]:80", "[344]", "[2AF::1]", "192.168.1.a", "192.168.1.0", "192.168.1.0:88" };
            illegalIps = IPUtil.CheckIPs(ips.ToArray());
            Assert.Equal("illegal ip: [344],192.168.1.a,192.168.1.0:88", illegalIps);

            ips = new List<string> { "[::1]", "[::1]:80", "[2AF::1]", "192.168.1.0" };
            illegalIps = IPUtil.CheckIPs(ips.ToArray());
            Assert.Equal("ok", illegalIps);
        }

        [Fact]
        public void CheckOK_Should_Succeed()
        {
            Assert.True(IPUtil.CheckOK("ok"));

            Assert.False(IPUtil.CheckOK("ok1"));
        }
    }
}
