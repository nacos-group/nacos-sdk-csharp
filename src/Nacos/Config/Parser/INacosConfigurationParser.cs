namespace Nacos.Config.Parser
{
    using System.Collections.Generic;

    public interface INacosConfigurationParser
    {
        IDictionary<string, string> Parse(string input);
    }
}
