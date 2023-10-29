namespace Nacos.Tests.Utils
{
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class NetUtilsTest
    {
        [Fact]
        public void LocalIP_Should_Succeed()
        {
            // Waiting for PR code to be merged
            var localIp = NetUtils.LocalIP();
        }
    }
}
