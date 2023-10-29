namespace Nacos.Tests.Utils
{
    using Nacos.Utils;
    using Xunit;

    public class NetUtilsTest
    {
        [Fact]
        public void LocalIP_Should_Succeed()
        {
            var localIp = NetUtils.LocalIP();
            Assert.NotNull(localIp);

            /*Environment.SetEnvironmentVariable("com.alibaba.nacos.client.local.ip", "192.169.1.1");
            localIp = NetUtils.LocalIP(true);
            Assert.Equal("192.169.1.1", localIp);

            Environment.SetEnvironmentVariable("com.alibaba.nacos.client.local.ip", string.Empty);
            Environment.SetEnvironmentVariable("com.alibaba.nacos.client.naming.local.ip", "192.169.1.2");
            localIp = NetUtils.LocalIP(true);
            Assert.Equal("192.169.1.2", localIp);*/
        }
    }
}
