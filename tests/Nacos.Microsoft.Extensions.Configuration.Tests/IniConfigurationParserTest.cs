namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using System;
    using Xunit;

    public class IniConfigurationParserTest
    {
        [Fact]
        public void IniTest()
        {
            var ini = @"
version=测试version

[ConnectionStrings]
Default=""Server=127.0.0.1;Port=3306;Database=demo;User Id=root;Password=123456;""

[AppSettings]
Str=val
num=1
arr:0=1
arr:1=2
arr:2=3

[AppSettings:subobj]
a = b
";

            var data = Nacos.IniParser.IniConfigurationStringParser.Instance.Parse(ini);

            Assert.NotNull(data);
            Assert.Equal(8, data.Count);
        }
    }
}
