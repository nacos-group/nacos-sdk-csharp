namespace Nacos.Tests.Config.Utils
{
    using Nacos.Config.Utils;
    using System;
    using Xunit;

    public class ContentUtilsTest
    {
        [Fact]
        public void Verify_Increment_Pub_Content_Should_Exception()
        {
            var content = "cont\rent";
            var ex = Assert.Throws<ArgumentException>(() => ContentUtils.VerifyIncrementPubContent(content));
            Assert.Equal("publish/delete content can not contain return and linefeed", ex.Message);

            content = "cont\nent";
            ex = Assert.Throws<ArgumentException>(() => ContentUtils.VerifyIncrementPubContent(content));
            Assert.Equal("publish/delete content can not contain return and linefeed", ex.Message);

            content = "cont\u0002ent";
            ex = Assert.Throws<ArgumentException>(() => ContentUtils.VerifyIncrementPubContent(content));
            Assert.Equal("publish/delete content can not contain(char)2", ex.Message);
        }

        [Fact]
        public void Get_Content_Identity_Should_Exception()
        {
            // TODO：There are some problems
            var content = "content";
            var ex = Assert.Throws<ArgumentException>(() => ContentUtils.GetContentIdentity(content));
            Assert.Equal("content does not contain separator", ex.Message);
        }

        [Fact]
        public void Truncate_Content_Should_Succeed()
        {
            var content = string.Empty;

            var tContent = ContentUtils.TruncateContent(content);
            Assert.Equal(string.Empty, tContent);

            for (var i = 0; i < 105; i++)
            {
                content += "c";
            }

            tContent = ContentUtils.TruncateContent(content);
            Assert.Equal(content.Substring(0, 100) + "...", tContent);

            content = content.Substring(0, 98);
            tContent = ContentUtils.TruncateContent(content);
            Assert.Equal(content, tContent);
        }
    }
}
