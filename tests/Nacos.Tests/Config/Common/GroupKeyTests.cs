namespace Nacos.Tests.Config.Common
{
    using Nacos.Config.Common;
    using Xunit;

    public class GroupKeyTests
    {
        [Fact]
        public void GetKey_Should_Succeed()
        {
            var key = GroupKey.GetKey("dataId%c+s", "group%c+s");
            Assert.Equal("dataId%25c%2Bs+group%25c%2Bs", key);
        }

        [Fact]
        public void GetKey_With_datumStr_Should_Succeed()
        {
            var key = GroupKey.GetKey("dataId%c+s", "group%c+s", "%datum+Str");
            Assert.Equal("dataId%25c%2Bs+group%25c%2Bs+%25datum%2BStr", key);
        }

        [Fact]
        public void GetKeyTenant_Should_Succeed()
        {
            var key = GroupKey.GetKeyTenant("dataId%c+s", "group%c+s", "%Tenant+");
            Assert.Equal("dataId%25c%2Bs+group%25c%2Bs+%25Tenant%2B", key);
        }

        [Fact]
        public void ParseKey_Should_Succeed()
        {
            // has dataId + group + tenant
            var texts = GroupKey.ParseKey("dataId%25c%2Bs+group%25c%2Bs+%25Tenant%2B");
            Assert.Equal("dataId%c+s", texts[0]);
            Assert.Equal("group%c+s", texts[1]);
            Assert.Equal("%Tenant+", texts[2]);

            // has dataId + group
            var texts1 = GroupKey.ParseKey("dataId%25c%2Bs+group%25c%2Bs");
            Assert.Equal("dataId%c+s", texts1[0]);
            Assert.Equal("group%c+s", texts1[1]);
            Assert.Null(texts1[2]);
        }
    }
}
