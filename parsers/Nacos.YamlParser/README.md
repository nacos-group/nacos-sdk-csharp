# nacos-sdk-csharp.YamlParser

nacos-sdk-csharp.YamlParser provides yaml/yml format config parser.

```csharp
builder.Host.UseNacosConfig(section: "NacosConfig", parser: Nacos.YamlParser.YamlConfigurationStringParser.Instance);

// or

builder.Host.ConfigureAppConfiguration((c, b) =>
{
    var config = b.Build();

    b.AddNacosV2Configuration(config.GetSection("NacosConfig"), parser: Nacos.YamlParser.YamlConfigurationStringParser.Instance);
});
```

## Links

* [Documentation](https://nacos-sdk-csharp.readthedocs.io/en/latest/)
* [nacos-sdk-csharp GitHub](https://github.com/nacos-group/nacos-sdk-csharp)