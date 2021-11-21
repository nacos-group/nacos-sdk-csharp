升级到1.0版本指引
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

背景
---------------

`nacos-sdk-csharp` v1.0.0 版本之前主要是对接 nacos server 1.x。

随着 nacos server 2.0.0 即将发布，`nacos-sdk-csharp` 也已经进行了一次重构和适配来对接。

由于 nacos server 从 1.x 到 2.x 客户端对接的协议有所变更，1.x主要是HTTP， 2.x主要是gRPC。

之前 sdk 主要是对 Open Api 进行了封装和补充了部分内容。

在 1.0.0 版本，sdk 的方法会对齐 java版的 sdk， 这里的调整会比较大，但是之前用的方法还会保留几个版本。

升级
---------------

大前提是 nacos server 版本已经是 2.0.0或更高版本。

修改引用的 nuget 包

.. code-block:: diff

        <ItemGroup>
    -       <PackageReference Include="nacos-sdk-csharp-unofficial" Version="0.8.5" />
    +       <PackageReference Include="nacos-sdk-csharp" Version="1.0.0-alphaxxxx" />
        </ItemGroup>

..  

    注： 包名已经移除了 `unofficial` 的后缀，1.0.0的版本目前还是prerelease，所以会有alpha的字样，
    正式发布后是没有的。如果依赖了扩展包，也需要做对应的处理。

核心的基础配置如下：

.. code-block:: json

    {
        "EndPoint": "",
        "ServerAddresses": [ "http://localhost:8848" ],
        "DefaultTimeOut": 15000,
        "Namespace": "cs",
        "ListenInterval": 1000,
        "AccessKey": "",
        "SecretKey": "",
        "UserName": "",
        "Password": "",
        "ConfigUseRpc": true,
        "NamingUseRpc": true,
        "NamingLoadCacheAtStart": ""
    }

只有当 `ConfigUseRpc` 和 `NamingUseRpc` 设置为true的时候，才会用 gRPC 去和 nacos server 交互， 反之还是 HTTP。

另外，Namespace 字段填写的值是控制台中的 命名空间Id，不是命名空间名称！！！！

配置变化
---------------

**SDK**

代码层使用的 `INacosConfigClient` 需要调整成 `INacosConfigService`， 提供了如下的方法。

.. code-block:: csharp

    Task<string> GetConfig(string dataId, string group, long timeoutMs);

    Task<string> GetConfigAndSignListener(string dataId, string group, long timeoutMs, IListener listener);

    Task AddListener(string dataId, string group, IListener listener);

    Task<bool> PublishConfig(string dataId, string group, string content);

    Task<bool> PublishConfig(string dataId, string group, string content, string type);

    Task<bool> RemoveConfig(string dataId, string group);

    Task RemoveListener(string dataId, string group, IListener listener);

    Task<string> GetServerStatus();

    Task ShutDown();

**集成ASP.NET Core**

主要是变更 `ConfigureAppConfiguration` 里面的 AddNacosConfiguration，其余的不需要变化。

.. code-block:: diff

        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
            {
                var c = builder.Build();
    -            builder.AddNacosConfiguration(c.GetSection("NacosConfig"));
    +            builder.AddNacosV2Configuration(c.GetSection("NacosConfig"));
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })


服务变化
---------------

**SDK**

代码层使用的 `INacosNamingClient` 需要调整成 `INacosNamingService`， 提供了如下的方法。


.. code-block:: csharp

    Task RegisterInstance(string serviceName, string ip, int port);

    Task RegisterInstance(string serviceName, string groupName, string ip, int port);

    Task RegisterInstance(string serviceName, string ip, int port, string clusterName);

    Task RegisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

    Task RegisterInstance(string serviceName, Instance instance);

    Task RegisterInstance(string serviceName, string groupName, Instance instance);

    Task DeregisterInstance(string serviceName, string ip, int port);

    Task DeregisterInstance(string serviceName, string groupName, string ip, int port);

    Task DeregisterInstance(string serviceName, string ip, int port, string clusterName);

    Task DeregisterInstance(string serviceName, string groupName, string ip, int port, string clusterName);

    Task DeregisterInstance(string serviceName, Instance instance);

    Task DeregisterInstance(string serviceName, string groupName, Instance instance);

    Task<List<Instance>> GetAllInstances(string serviceName);

    Task<List<Instance>> GetAllInstances(string serviceName, string groupName);

    Task<List<Instance>> GetAllInstances(string serviceName, bool subscribe);

    Task<List<Instance>> GetAllInstances(string serviceName, string groupName, bool subscribe);

    Task<List<Instance>> GetAllInstances(string serviceName, List<string> clusters);

    Task<List<Instance>> GetAllInstances(string serviceName, string groupName, List<string> clusters);

    Task<List<Instance>> GetAllInstances(string serviceName, List<string> clusters, bool subscribe);

    Task<List<Instance>> GetAllInstances(string serviceName, string groupName, List<string> clusters, bool subscribe);

    Task<List<Instance>> SelectInstances(string serviceName, bool healthy);

    Task<List<Instance>> SelectInstances(string serviceName, string groupName, bool healthy);

    Task<List<Instance>> SelectInstances(string serviceName, bool healthy, bool subscribe);

    Task<List<Instance>> SelectInstances(string serviceName, string groupName, bool healthy, bool subscribe);

    Task<List<Instance>> SelectInstances(string serviceName, List<string> clusters, bool healthy);

    Task<List<Instance>> SelectInstances(string serviceName, string groupName, List<string> clusters, bool healthy);

    Task<List<Instance>> SelectInstances(string serviceName, List<string> clusters, bool healthy, bool subscribe);

    Task<List<Instance>> SelectInstances(string serviceName, string groupName, List<string> clusters, bool healthy, bool subscribe);

    Task<Instance> SelectOneHealthyInstance(string serviceName);

    Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName);

    Task<Instance> SelectOneHealthyInstance(string serviceName, bool subscribe);

    Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, bool subscribe);

    Task<Instance> SelectOneHealthyInstance(string serviceName, List<string> clusters);

    Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, List<string> clusters);

    Task<Instance> SelectOneHealthyInstance(string serviceName, List<string> clusters, bool subscribe);

    Task<Instance> SelectOneHealthyInstance(string serviceName, string groupName, List<string> clusters, bool subscribe);

    Task Subscribe(string serviceName, IEventListener listener);

    Task Subscribe(string serviceName, string groupName, IEventListener listener);

    Task Subscribe(string serviceName, List<string> clusters, IEventListener listener);

    Task Subscribe(string serviceName, string groupName, List<string> clusters, IEventListener listener);

    Task Unsubscribe(string serviceName, IEventListener listener);

    Task Unsubscribe(string serviceName, string groupName, IEventListener listener);

    Task Unsubscribe(string serviceName, List<string> clusters, IEventListener listener);

    Task Unsubscribe(string serviceName, string groupName, List<string> clusters, IEventListener listener);

    Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize);

    Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, string groupName);

    Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, AbstractSelector selector);

    Task<ListView<string>> GetServicesOfServer(int pageNo, int pageSize, string groupName, AbstractSelector selector);

    Task<List<ServiceInfo>> GetSubscribeServices();

    Task<string> GetServerStatus();

    Task ShutDown();


**集成ASP.NET Core**

调整 `Startup`

.. code-block:: diff

            public void ConfigureServices(IServiceCollection services)
            {
    -            services.AddNacosAspNetCore(Configuration);
    +            services.AddNacosAspNet(Configuration);
                services.AddControllers();
            }

用到 `INacosServerManager` 的地方需要换成 `INacosNamingService`。

具体使用如下：

.. code-block:: diff

    - var baseUrl = await _serverManager.GetServerAsync("App2");

    + var instance = await _svc.SelectOneHealthyInstance("App2", "DEFAULT_GROUP");
    + var host = $"{instance.Ip}:{instance.Port}";

    + var baseUrl = instance.Metadata.TryGetValue("secure", out _)
    +    ? $"https://{host}"
    +    : $"http://{host}";

服务的配置，在原先的基础上添加了， **InstanceEnabled**， **Ephemeral**， **Secure** 三个内容。


