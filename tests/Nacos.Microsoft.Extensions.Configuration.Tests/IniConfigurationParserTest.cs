namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using System;
    using Xunit;

    [Trait("Category", "all")]
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

;sdf
#abc
/123

[AppSettings:subobj]
a = b
";

            var data = Nacos.IniParser.IniConfigurationStringParser.Instance.Parse(ini);

            Assert.NotNull(data);
            Assert.Equal(8, data.Count);
        }

        [Fact]
        public void IniTest_Should_ThrowException_When_Miss_Equal()
        {
            var ini = @"
version
";

            Assert.Throws<FormatException>(() => Nacos.IniParser.IniConfigurationStringParser.Instance.Parse(ini));
        }

        [Fact]
        public void IniTest_Should_ThrowException_When_Contains_DuplicateKey()
        {
            var ini = @"
version=1
version=2
";

            Assert.Throws<FormatException>(() => Nacos.IniParser.IniConfigurationStringParser.Instance.Parse(ini));
        }

        [Fact]
        public void IniTest_ThreadSafe()
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

            System.Threading.Tasks.Parallel.For(0, 20, x =>
            {
                var data = Nacos.IniParser.IniConfigurationStringParser.Instance.Parse(ini);

                Assert.NotNull(data);
                Assert.Equal(8, data.Count);
            });
        }
    }
}
