namespace Nacos.Microsoft.Extensions.Configuration
{
    using System.Collections.Generic;

    public interface INacosConfigurationParser
    {
        IDictionary<string, string> Parse(string input);
    }
}
