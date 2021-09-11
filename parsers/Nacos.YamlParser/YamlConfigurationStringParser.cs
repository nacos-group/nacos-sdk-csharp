namespace Nacos.YamlParser
{
    using Microsoft.Extensions.Configuration;
    using Nacos.V2;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using YamlDotNet.RepresentationModel;

    public class YamlConfigurationStringParser : INacosConfigurationParser
    {
        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;

        public static YamlConfigurationStringParser Instance = new YamlConfigurationStringParser();

        public IDictionary<string, string> Parse(string input)
        {
            _data.Clear();
            _context.Clear();

            using var reader = new StringReader(input);

            var yaml = new YamlStream();
            yaml.Load(reader);

            if (yaml.Documents.Any())
            {
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                VisitYamlMappingNode(mapping);
            }

            return _data;
        }

        private void VisitYamlNodePair(KeyValuePair<YamlNode, YamlNode> yamlNodePair)
        {
            var context = ((YamlScalarNode)yamlNodePair.Key).Value;
            VisitYamlNode(context, yamlNodePair.Value);
        }

        private void VisitYamlNode(string context, YamlNode node)
        {
            if (node is YamlScalarNode scalarNode)
            {
                VisitYamlScalarNode(context, scalarNode);
            }

            if (node is YamlMappingNode mappingNode)
            {
                VisitYamlMappingNode(context, mappingNode);
            }

            if (node is YamlSequenceNode sequenceNode)
            {
                VisitYamlSequenceNode(context, sequenceNode);
            }
        }

        private void VisitYamlScalarNode(string context, YamlScalarNode yamlValue)
        {
            EnterContext(context);
            var currentKey = _currentPath;

            if (_data.ContainsKey(currentKey))
            {
                throw new FormatException($"A duplicate key '{currentKey}' was found.");
            }

            _data[currentKey] = IsNullValue(yamlValue) ? null : yamlValue.Value;
            ExitContext();
        }

        private void VisitYamlMappingNode(YamlMappingNode node)
        {
            foreach (var yamlNodePair in node.Children)
            {
                VisitYamlNodePair(yamlNodePair);
            }
        }

        private void VisitYamlMappingNode(string context, YamlMappingNode yamlValue)
        {
            EnterContext(context);

            VisitYamlMappingNode(yamlValue);

            ExitContext();
        }

        private void VisitYamlSequenceNode(string context, YamlSequenceNode yamlValue)
        {
            EnterContext(context);

            VisitYamlSequenceNode(yamlValue);

            ExitContext();
        }

        private void VisitYamlSequenceNode(YamlSequenceNode node)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                VisitYamlNode(i.ToString(), node.Children[i]);
            }
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private bool IsNullValue(YamlScalarNode yamlValue)
        {
            return yamlValue.Style == YamlDotNet.Core.ScalarStyle.Plain
                && (
                    yamlValue.Value == "~"
                    || yamlValue.Value == "null"
                    || yamlValue.Value == "Null"
                    || yamlValue.Value == "NULL");
        }
    }
}
