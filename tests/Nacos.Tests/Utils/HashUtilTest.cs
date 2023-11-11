namespace Nacos.Tests.Utils
{
    using Nacos.Utils;
    using Xunit;

    public class HashUtilTest
    {
        [Fact]
        public void GetMd5_Should_Succeed()
        {
            var value = "1234567890";
            var md5 = HashUtil.GetMd5(value);
            Assert.Equal("e807f1fcf82d132f9bb018ca6738a19f", md5);
        }

        [Fact]
        public void GetHMACSHA1_Should_Succeed()
        {
            var value = "1234567890";
            var key = "MACSHA1-Key";
            var hash = HashUtil.GetHMACSHA1(value, key);
            Assert.Equal("RmB6EBR/z3rfmx0ELUOW8Xx4pzA=", hash);
        }
    }
}
