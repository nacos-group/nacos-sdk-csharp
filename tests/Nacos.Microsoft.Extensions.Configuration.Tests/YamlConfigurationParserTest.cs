namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using System;
    using Xunit;

    [Trait("Category", "all")]
    public class YamlConfigurationParserTest
    {
        [Fact]
        public void YamlTest()
        {
            var yaml = @"---
ConnectionStrings: 
  Default: Server=127.0.0.1;Port=3306;Database=demo;User Id=root;Password=123456; 
version: 测试version
AppSettings: 
  Str: val
  num: 1
  arr: 
  - 1
  - 2
  - 3
  subobj: 
    a: b";

            var data = Nacos.YamlParser.YamlConfigurationStringParser.Instance.Parse(yaml);

            Assert.NotNull(data);
            Assert.Equal(8, data.Count);
        }

        [Fact]
        public void YamlTest_Should_ThrowException_When_Contains_DuplicateKey()
        {
            var yaml = @"
 a: 
  - 1
 a:0: 123
                         ";

            Assert.Throws<FormatException>(() => Nacos.YamlParser.YamlConfigurationStringParser.Instance.Parse(yaml));
        }

        [Fact]
        public void YamlTest_ThreadSafe_Iss176()
        {
            var yaml = @"---
ConnectionStrings: 
  Default: Server=127.0.0.1;Port=3306;Database=demo;User Id=root;Password=123456; 
version: 测试version
AppSettings: 
  Str: val
  num: 1
  arr: 
  - 1
  - 2
  - 3
  subobj: 
    a: b";

            System.Threading.Tasks.Parallel.For(0, 20, x =>
            {
                var data = Nacos.YamlParser.YamlConfigurationStringParser.Instance.Parse(yaml);

                Assert.NotNull(data);
                Assert.Equal(8, data.Count);
            });
        }
    }
}
