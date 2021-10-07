namespace Nacos.System.Configuration
{
    using global::System;
    using global::System.Configuration;
    using Nacos.Microsoft.Extensions.Configuration;
    using Nacos.V2;

    public class ConfigListener : ConfigurationSection
    {
        /// <summary>
        /// Configuration ID
        /// </summary>
        [ConfigurationProperty("dataId", IsRequired = false)]
        public string DataId => this["dataId"]?.ToString();

        /// <summary>
        /// Configuration group
        /// </summary>
        [ConfigurationProperty("group", DefaultValue = "DEFAULT_GROUP", IsRequired = false)]
        public string Group => this["group"]?.ToString();

        [ConfigurationProperty("parserType", IsRequired = false)]
        public string ParserType => this["parserType"] as string;

        /// <summary>
        /// The configuration parser, default is json
        /// </summary>
        public INacosConfigurationParser NacosConfigurationParser
        {
            get
            {
                _parser ??= new Lazy<INacosConfigurationParser>(() =>
                {
                    if (string.IsNullOrWhiteSpace(ParserType)) return null;

                    var type = Type.GetType(ParserType);
                    if (type == null) throw new TypeLoadException("不能找到类型" + ParserType);

                    return (INacosConfigurationParser)Activator.CreateInstance(type);
                });

                return _parser.Value ?? DefaultJsonConfigurationStringParser.Instance;
            }
        }

        private Lazy<INacosConfigurationParser> _parser;
    }
}
