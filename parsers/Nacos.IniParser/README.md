# nacos-sdk-csharp.IniParser

nacos-sdk-csharp.IniParser provides ini format config parser.

```csharp
builder.Host.UseNacosConfig(section: "NacosConfig", parser: Nacos.IniParser.IniConfigurationStringParser.Instance);

// or

builder.Host.ConfigureAppConfiguration((c, b) =>
{
    var config = b.Build();

    b.AddNacosV2Configuration(config.GetSection("NacosConfig"), parser: Nacos.IniParser.IniConfigurationStringParser.Instance);
});
```

## Links

* [Documentation](https://nacos-sdk-csharp.readthedocs.io/en/latest/)
* [nacos-sdk-csharp GitHub](https://github.com/nacos-group/nacos-sdk-csharp)