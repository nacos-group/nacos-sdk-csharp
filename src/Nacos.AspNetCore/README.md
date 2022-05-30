# nacos-sdk-csharp.AspNetCore

nacos-sdk-csharp.AspNetCore provides service registration and discovery With ASP.NET Core.

```csharp
builder.Services.AddNacosAspNet(x =>
{
    x.ServerAddresses = new List<string> { "http://localhost:8848/" };    
    x.Namespace = "cs";
    x.ServiceName = "App2";
    x.GroupName = "DEFAULT_GROUP";
    x.ClusterName = "DEFAULT";
    x.Ip = "";
    x.PreferredNetworks = "";
    x.Port = 0;
    x.Weight = 100;
    x.RegisterEnabled = true;
    x.InstanceEnabled = true;
    x.Ephemeral = true;
    x.Secure = false;
});

// or

builder.Services.AddNacosAspNet(builder.Configuration);
```

## Links

* [Documentation](https://nacos-sdk-csharp.readthedocs.io/en/latest/)
* [nacos-sdk-csharp GitHub](https://github.com/nacos-group/nacos-sdk-csharp)