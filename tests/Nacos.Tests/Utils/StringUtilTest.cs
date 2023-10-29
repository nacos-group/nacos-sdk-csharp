namespace Nacos.Tests.Utils
{
    using Nacos.Utils;
    using System.Linq;
    using Xunit;

    public class StringUtilTest
    {
        [Fact]
        public void SplitByString_Should_Succeed()
        {
            var splits = "aa@bb@cc-/@dd".SplitByString("@");
            Assert.Equal(4, splits.Count());
            Assert.Equal("cc-/", splits[2]);

            splits = "aa@@cc-/@dd".SplitByString("@");
            Assert.Equal(3, splits.Count());
            Assert.Equal("dd", splits[2]);
        }

        [Fact]
        public void IsNullOrWhiteSpace_IsNotNullOrWhiteSpace_Should_Succeed()
        {
            string str = null;
            Assert.True(str.IsNullOrWhiteSpace());

            str = "";
            Assert.True(str.IsNullOrWhiteSpace());

            str = "2";
            Assert.False(str.IsNullOrWhiteSpace());

            str = null;
            Assert.False(str.IsNotNullOrWhiteSpace());

            str = "";
            Assert.False(str.IsNotNullOrWhiteSpace());

            str = "2";
            Assert.True(str.IsNotNullOrWhiteSpace());
        }

        [Fact]
        public void ToEncoded_ecode_Should_Succeed()
        {
            var content = "ab@cd$ef%gl-j/k";
            var encoded = content.ToEncoded();
            Assert.Equal("ab%40cd%24ef%25gl-j%2Fk", encoded);

            var decode = encoded.ToDecode();
            Assert.Equal(content, decode);
        }
    }
}
