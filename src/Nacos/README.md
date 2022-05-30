# nacos-sdk-csharp 

nacos-sdk-csharp provides basic nacos operation.

```csharp
// config
builder.Services.AddNacosV2Config(x =>
{
    x.ServerAddresses = new List<string> { "http://localhost:8848/" };
    x.Namespace = "cs";
    x.UserName = "nacos";
    x.Password = "nacos";
});

// naming
builder.Services.AddNacosV2Naming(x =>
{
    x.ServerAddresses = new List<string> { "http://localhost:8848/" };
    x.Namespace = "cs";
    x.UserName = "nacos";
    x.Password = "nacos";
});

// or 

builder.Services.AddNacosV2Config(builder.Configuration);

builder.Services.AddNacosV2Naming(builder.Configuration);
```

```csharp
private readonly INacosConfigService _config;
private readonly INacosNamingService _naming;

public TheCtor(INacosConfigService config, INacosNamingService naming)
{
    _config = config;
    _naming = naming;
}
```

## Links

* [Documentation](https://nacos-sdk-csharp.readthedocs.io/en/latest/)
* [nacos-sdk-csharp GitHub](https://github.com/nacos-group/nacos-sdk-csharp)