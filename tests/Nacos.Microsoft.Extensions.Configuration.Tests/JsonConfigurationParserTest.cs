namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using System;
    using Xunit;

    public class JsonConfigurationParserTest
    {
        [Fact]
        public void JsonTest()
        {
            var json = @"{""ConnectionStrings"":{""Default"":""Server=127.0.0.1;Port=3306;Database=demo;User Id=root;Password=123456; ""},""version"":""≤‚ ‘version"",""AppSettings"":{""Str"":""val"",""num"":1,""arr"":[1,2,3],""subobj"":{""a"":""b""}}}";

            var data = DefaultJsonConfigurationStringParser.Instance.Parse(json);

            Assert.NotNull(data);
        }
    }
}
