# nacos-sdk-csharp 　　　　　   　　   　　　[中文](./README.zh-cn.md)

csharp(dotnet core) implementation of [nacos](https://nacos.io/) OpenAPI.

![Build](https://github.com/nacos-group/nacos-sdk-csharp/workflows/Build/badge.svg) ![Release](https://github.com/nacos-group/nacos-sdk-csharp/workflows/Release/badge.svg) ![](https://img.shields.io/nuget/v/nacos-sdk-csharp.svg)  ![](https://img.shields.io/nuget/vpre/nacos-sdk-csharp.svg) ![](https://img.shields.io/nuget/dt/nacos-sdk-csharp) ![](https://img.shields.io/github/license/nacos-group/nacos-sdk-csharp)

![](./media/prj.png)

## Installation

Choose a package that you need.

```bash
dotnet add package nacos-sdk-csharp
dotnet add package nacos-sdk-csharp.AspNetCore
dotnet add package nacos-sdk-csharp.Extensions.Configuration
dotnet add package nacos-sdk-csharp.YamlParser
dotnet add package nacos-sdk-csharp.IniParser
```

> NOTE: The packages' name has remove the suffix `unofficial`.

## Features

- Basic OpenApi Usages
- Integrate ASP.NET Core Configuration System
- Service Registration and Discovery With ASP.NET Core
- Integrate With Aliyun MSE/ACM
- ...

Find more information on the documents pages:

https://nacos-sdk-csharp.readthedocs.io/en/latest/

## Basic Usage

### Simple Configuration Usage

1. Configure in `Program.cs`

```cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, builder) =>
        {
            var c = builder.Build();

           // read configuration from config files
            // it will use default json parser to parse the configuration store in nacos server.
            builder.AddNacosV2Configuration(c.GetSection("NacosConfig"));
            // you also can specify ini or yaml parser as well.
            // builder.AddNacosV2Configuration(c.GetSection("NacosConfig"), Nacos.IniParser.IniConfigurationStringParser.Instance);
            // builder.AddNacosV2Configuration(c.GetSection("NacosConfig"), Nacos.YamlParser.YamlConfigurationStringParser.Instance);
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
```

2. Modify `appsettings.json`

```JSON
{
  "NacosConfig": {
    "Listeners": [
      {
        "Optional": false,
        "DataId": "common",
        "Group": "DEFAULT_GROUP"
      },
      {
        "Optional": false,
        "DataId": "demo",
        "Group": "DEFAULT_GROUP"
      }
    ],    
    "Namespace": "csharp-demo",  // Please set the value of Namespace ID !!!!!!!!
    "ServerAddresses": [ "http://localhost:8848/" ],
    "UserName": "test2",
    "Password": "123456",
    "AccessKey": "",
    "SecretKey": "",
    "EndPoint": "acm.aliyun.com",
    "ConfigFilterAssemblies": ["YouPrefix.AssemblyName"],
    "ConfigFilterExtInfo": "some ext infomation"
  }
}
```

3. Use via .NET Core's Way

```cs
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppSettings _settings;
    private readonly AppSettings _sSettings;
    private readonly AppSettings _mSettings;
    
    public ConfigController(
        IConfiguration configuration,
        IOptions<AppSettings> options,
        IOptionsSnapshot<AppSettings> sOptions,
        IOptionsMonitor<AppSettings> _mOptions
        )
    {
        _logger = logger;
        _configuration = configuration;
        _settings = options.Value;
        _sSettings = sOptions.Value;
        _mSettings = _mOptions.CurrentValue;
    }

    [HttpGet]
    public string Get()
    {
        // ....
       
        return "ok";
    }

}
```

### Service Registration and Discovery

1. Service Registration

Configure in `Program.cs`

```cs
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // ...

        services.AddNacosAspNet(Configuration, "nacos");
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // ...
    }
}
```

Modify `appsettings.json`

```JSON
"nacos": {
    "EndPoint": "sub-domain.aliyun.com:8080",
    "ServerAddresses": [ "http://localhost:8848" ],
    "DefaultTimeOut": 15000,
    "Namespace": "cs", // Please set the value of Namespace ID !!!!!!!!
    "ListenInterval": 1000,
    "ServiceName": "App1",
    "GroupName": "DEFAULT_GROUP",
    "ClusterName": "DEFAULT",
    "Ip": "",
    "PreferredNetworks": "", // select an IP that matches the prefix as the service registration IP
    "Port": 0,
    "Weight": 100,
    "RegisterEnabled": true,
    "InstanceEnabled": true,
    "Ephemeral": true,
    "Secure": false,
    "AccessKey": "",
    "SecretKey": "",
    "UserName": "",
    "Password": "",
    "ConfigUseRpc": true,
    "NamingUseRpc": true,
    "NamingLoadCacheAtStart": "",       
    "LBStrategy": "WeightRandom", //WeightRandom WeightRoundRobin
    "Metadata": {
      "aa": "bb",
      "cc": "dd"
    }
  }
```

2. Service Discovery

```cs
[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly Nacos.V2.INacosNamingService _svc;

    public ValuesController(Nacos.V2.INacosNamingService svc)
    {
        _svc = svc;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {        
        // need to know the service name.
        var instance = await _svc.SelectOneHealthyInstance("App2", "DEFAULT_GROUP");
        var host = $"{instance.Ip}:{instance.Port}";

        var baseUrl = instance.Metadata.TryGetValue("secure", out _)
            ? $"https://{host}"
            : $"http://{host}";
                    
        if(string.IsNullOrWhiteSpace(baseUrl))
        {
            return "empty";
        }

        var url = $"{baseUrl}/api/values";

        using (HttpClient client = new HttpClient())
        {
            var result = await client.GetAsync(url);
            return await result.Content.ReadAsStringAsync();
        }
    }
}
```
