namespace Nacos.IniParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.Extensions.Configuration;
    using Nacos.V2;

    public class IniConfigurationStringParser : INacosConfigurationParser
    {
        public IniConfigurationStringParser()
        {
        }

        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static IniConfigurationStringParser Instance = new IniConfigurationStringParser();

        public IDictionary<string, string> Parse(string input)
          => new IniConfigurationStringParser().ParseString(input);

        public IDictionary<string, string> ParseString(string input)
        {
            _data.Clear();

            byte[] array = Encoding.UTF8.GetBytes(input);

            using (MemoryStream stream = new MemoryStream(array))
            {
                using (var reader = new StreamReader(stream))
                {
                    string sectionPrefix = string.Empty;

                    while (reader.Peek() != -1)
                    {
                        string rawLine = reader.ReadLine();
                        string line = rawLine.Trim();

                        // Ignore blank lines
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        // Ignore comments
                        if (line[0] == ';' || line[0] == '#' || line[0] == '/')
                        {
                            continue;
                        }

                        // [Section:header]
                        if (line[0] == '[' && line[line.Length - 1] == ']')
                        {
                            // remove the brackets
                            sectionPrefix = line.Substring(1, line.Length - 2) + ConfigurationPath.KeyDelimiter;
                            continue;
                        }

                        // key = value OR "value"
                        int separator = line.IndexOf('=');
                        if (separator < 0)
                        {
                            throw new FormatException($"Unrecognized line format: '{rawLine}'");
                        }

                        string key = sectionPrefix + line.Substring(0, separator).Trim();
                        string value = line.Substring(separator + 1).Trim();

                        // Remove quotes
                        if (value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"')
                        {
                            value = value.Substring(1, value.Length - 2);
                        }

                        if (_data.ContainsKey(key))
                        {
                            throw new FormatException($"A duplicate key '{key}' was found.");
                        }

                        _data[key] = value;
                    }
                }
            }

            return _data;
        }
    }
}
