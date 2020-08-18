namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using System;
    using Xunit;

    public class IniConfigurationParserTest
    {
        [Fact]
        public void IniTest()
        {
            var ini = @"";

            var data = Nacos.IniParser.IniConfigurationStringParser.Instance.Parse(ini);

            Assert.NotNull(data);
        }
    }
}
