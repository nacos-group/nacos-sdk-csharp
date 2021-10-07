快速上手
===============

`nacos-sdk-csharp` 是基于 C# ( dotnet core ) 实现 `nacos <https://nacos.io/>`_ 的版本

安装Nuget包
^^^^^^^^^^^^^

选择您需要的包。

.. code-block:: bash

    dotnet add package nacos-sdk-csharp
    dotnet add package nacos-sdk-csharp.AspNetCore
    dotnet add package nacos-sdk-csharp.Extensions.Configuration
    dotnet add package nacos-sdk-csharp.YamlParser
    dotnet add package nacos-sdk-csharp.IniParser


::

    注: 从1.0.0版本之后，包名里面的 `unofficial` 后缀已经被移除，
    带 `unofficial` 的包已经不再维护更新，请尽早更新到最新版本。

功能特性
^^^^^^^^^^^^^

- 基本的Open Api接口封装
- 集成ASP.NET Core的配置系统
- 简易ASP.NET Core的服务注册和发现
- 和阿里云应用配置管理(Application Configuration Management，简称 ACM)集成使用
- ...

简易用法
^^^^^^^^^^^^^

配置
---------

1. 在 `Program.cs` 进行如下配置

.. code-block:: csharp

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
            {
                var c = builder.Build();

                // 从配置文件读取Nacos相关配置
                // 默认会使用JSON解析器来解析存在Nacos Server的配置
                builder.AddNacosV2Configuration(c.GetSection("NacosConfig"));
                // 也可以按需使用ini或yaml的解析器
                // builder.AddNacosV2Configuration(c.GetSection("NacosConfig"), Nacos.IniParser.IniConfigurationStringParser.Instance);
                // builder.AddNacosV2Configuration(c.GetSection("NacosConfig"), Nacos.YamlParser.YamlConfigurationStringParser.Instance);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })


2. 修改 `appsettings.json`

.. code-block:: json

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
            "Namespace": "csharp-demo",
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

3. 用原生的.NET Core方式来读取Nacos配置

.. code-block:: csharp

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
    

服务注册和发现
------------------

1. 服务注册

在 `Program.cs` 中配置

.. code-block:: csharp

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


修改 `appsettings.json`

.. code-block:: json

    "nacos": {
        "EndPoint": "sub-domain.aliyun.com:8080",
        "ServerAddresses": [ "http://localhost:8848" ],
        "DefaultTimeOut": 15000,
        "Namespace": "cs",
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


2. 服务发现

.. code-block:: csharp

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
            // 这里需要知道被调用方的服务名
            var instance = await _svc.SelectOneHealthyInstance("App2", "DEFAULT_GROUP")
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

