namespace Nacos.V2
{
    using System.Collections.Generic;

    public interface INacosConfigurationParser
    {
        IDictionary<string, string> Parse(string input);
    }
}
