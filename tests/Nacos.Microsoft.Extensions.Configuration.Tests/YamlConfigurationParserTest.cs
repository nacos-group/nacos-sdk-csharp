namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using System;
    using Xunit;

    public class YamlConfigurationParserTest
    {
        [Fact]
        public void YamlTest()
        {
            var yaml = @"";

            var data = Nacos.YamlParser.YamlConfigurationStringParser.Instance.Parse(yaml);

            Assert.NotNull(data);
        }
    }
}
