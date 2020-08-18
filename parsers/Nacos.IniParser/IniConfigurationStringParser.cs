namespace Nacos.IniParser
{
    using System.Collections.Generic;
    using Nacos.Config;

    public class IniConfigurationStringParser : INacosConfigurationParser
    {
        public static IniConfigurationStringParser Instance = new IniConfigurationStringParser();

        public IDictionary<string, string> Parse(string input)
        {
            throw new System.NotImplementedException();
        }
    }
}
