# nacos-sdk-csharp.Extensions.Configuration

nacos-sdk-csharp.Extensions.Configuration provides integration with ASP.NET Core Configuration System.

```csharp
builder.Host.UseNacosConfig(section: "NacosConfig");

// or

builder.Host.ConfigureAppConfiguration((c, b) =>
{
    var config = b.Build();

    // read configuration from config files
    // default is json
    // b.AddNacosV2Configuration(config.GetSection("NacosConfig"));
    b.AddNacosV2Configuration(config.GetSection("NacosConfig"));

    // specify ini or yaml
    b.AddNacosV2Configuration(config.GetSection("NacosConfig"), parser: Nacos.IniParser.IniConfigurationStringParser.Instance);
    b.AddNacosV2Configuration(config.GetSection("NacosConfig"), parser: Nacos.YamlParser.YamlConfigurationStringParser.Instance);
});
```

## Links

* [Documentation](https://nacos-sdk-csharp.readthedocs.io/en/latest/)
* [nacos-sdk-csharp GitHub](https://github.com/nacos-group/nacos-sdk-csharp)