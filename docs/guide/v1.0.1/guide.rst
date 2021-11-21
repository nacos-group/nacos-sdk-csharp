Guidelines for upgrading to version 1.0
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Background
---------------

Before `nacos-sdk-csharp` v1.0.0, it was mainly connected to Nacos server 1. X.

With the release of Nacos server 2.0.0, `nacos-sdk-csharp` has been reconstructed and adapted to dock.

Because the protocol of Nacos server from 1.X to 2.X is changed, 1.X is mainly HTTP, 2.X is mainly grpc.

Before, the SDK mainly encapsulated and supplemented some contents of open API.

In version 1.0.0, the SDK method will align with the Java version of the SDK. The adjustment here will be relatively large, but several versions of the previous method will be retained.

Upgrade
------------


The premise is that the version of nacos server is 2.0.0 or higher.

Modify the referenced nuget package

.. code-block:: diff

        <ItemGroup>
    -       <PackageReference Include="nacos-sdk-csharp-unofficial" Version="0.8.5" />
    +       <PackageReference Include="nacos-sdk-csharp" Version="1.0.0-alphaxxxx" />
        </ItemGroup>

..  

    NOTE： The suffix "unofficial" has been removed from the package name. The version 1.0.0 is still prereleased, 
    so there will be alpha, which will not be available after the official release. 
    If you depend on the extension package, you need to do the corresponding processing.


The basic configuration is as follows:

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


Only when 'ConfigUseRpc' and 'NamingUseRpc' are set to true, gPRC will be used to interact with Nacos server, otherwise HTTP will be used.

And `Namespace` should be set to the Namespace Id in the nacos console, not the Namespace name!!!!!!

Configuration changes
--------------------------

**SDK**

Adjust `INacosConfigClient` to `INacosConfigService`， and it providers the following methods.

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


**Integrate ASP.NET Core**

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



Service changes
--------------------------

**SDK**

Adjust `INacosNamingClient` to `INacosNamingService`， and it providers the following methods.

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


**Integrate ASP.NET Core**

Modify `Startup`

.. code-block:: diff

            public void ConfigureServices(IServiceCollection services)
            {
    -            services.AddNacosAspNetCore(Configuration);
    +            services.AddNacosAspNet(Configuration);
                services.AddControllers();
            }


From `INacosServerManager` to `INacosNamingService`。

Details：

.. code-block:: diff

    - var baseUrl = await _serverManager.GetServerAsync("App2");

    + var instance = await _svc.SelectOneHealthyInstance("App2", "DEFAULT_GROUP");
    + var host = $"{instance.Ip}:{instance.Port}";

    + var baseUrl = instance.Metadata.TryGetValue("secure", out _)
    +    ? $"https://{host}"
    +    : $"http://{host}";


On the basis of the original service configuration, three options are added: **InstanceEnabled**, **Ephemeral**, **Secure**.