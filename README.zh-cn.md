# nacos-sdk-csharp 　　　　   　　　　   　　[English](./README.md)

基于C#(dotnet core)实现 [nacos](https://nacos.io/) OpenAPI 的非官方版本

![](https://img.shields.io/nuget/v/nacos-sdk-csharp-unofficial.svg)

![](./media/prj.png)

## CI构建状态

| Platform | Build Server | Master Status  |
|--------- |------------- |---------|
| Github Action   | Linux/OSX |![nacos-sdk-csharp CI](https://github.com/catcherwong/nacos-sdk-csharp/workflows/nacos-sdk-csharp%20CI/badge.svg)
 |

## 安装Nuget包

```bash
dotnet add package nacos-sdk-csharp-unofficial
```

## 功能特性

- 基本的Open Api接口封装
- 集成ASP.NET Core的配置系统
- 简易ASP.NET Core的服务注册和发现
- 和阿里云应用配置管理(Application Configuration Management，简称 ACM)集成使用
- ...

## 简易用法

### 配置

1. 在 `Program.cs` 进行如下配置

```cs
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, builder) =>
        {
            var c = builder.Build();

            // read configuration from config files
            builder.AddNacosConfiguration(c.GetSection("NacosConfig"));
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
```

2. 修改 `appsettings.json`

```JSON
{
  "NacosConfig": {
    "Optional": false,
    "DataId": "msconfigapp",
    "Group": "",
    "Tenant": "f47e0ae1-982a-4a64-aea3-52506492a3d4",
    "ServerAddresses": [ "http://localhost:8848/" ],
    "UserName": "test2",
    "Password": "123456",
    "AccessKey": "",
    "SecretKey": "",
    "EndPoint": "acm.aliyun.com"
  }
}
```

3. 用原生的.NET Core方式来读取Nacos配置

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

### 服务注册和发现

1. 服务注册

在 `Program.cs` 中配置

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

        services.AddNacosAspNetCore(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // ...
    }
}
```

修改 `appsettings.json`

```JSON
"nacos": {
    "ServerAddresses": [ "http://localhost:8848" ],
    "DefaultTimeOut": 15000,
    "Namespace": "",
    "ListenInterval": 1000,
    "ServiceName": "App1"
  }
```

2. 服务发现

```cs
[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly INacosServerManager _serverManager;

    public ValuesController(INacosServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {        
        // 这里需要知道被调用方的服务名
        // 目前获取服务地址是随机的方式，后续会加入更多负载算法.
        var baseUrl = await _serverManager.GetServerAsync("App2");
                    
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
