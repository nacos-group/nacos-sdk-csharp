namespace Nacos.Tests.Parsers
{
    using Nacos.IniParser;
    using Nacos.YamlParser;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class ParserTests
    {
        [Fact]
        public void Ini_Parser_Should_Succeed()
        {
            var ini = @"[http]
doamin=nacos.io
port=80
path=/zh-cn/docs/v2
 
[database]
server = sqlserver
user = myusername
password = mypassword
";

            var iniParser = new IniConfigurationStringParser();
            var configs = iniParser.Parse(ini);

            Assert.Equal(6, configs.Count());
            Assert.Equal("nacos.io", configs["http:doamin"]);
            Assert.Equal("/zh-cn/docs/v2", configs["http:path"]);
            Assert.Equal("sqlserver", configs["database:server"]);
            Assert.Equal("mypassword", configs["database:password"]);
        }

        [Fact]
        public void Yaml_Parser_Should_Succeed()
        {
            var yaml = @"---
yaml:
- slim and flexible
- better for configuration
object:
  key: value
  array:
  - null_value: 
  - boolean: true
  - integer: 1
content: |-
  Or we
  can auto
  convert line breaks
  to save space";

            var yamlParser = new YamlConfigurationStringParser();
            var configs = yamlParser.Parse(yaml);

            Assert.Equal(7, configs.Count());
            Assert.Equal("better for configuration", configs["yaml:1"]);
            Assert.Equal("value", configs["object:key"]);
            Assert.Equal("true", configs["object:array:1:boolean"]);
            Assert.Equal("Or we\ncan auto\nconvert line breaks\nto save space", configs["content"]);
        }
    }
}
