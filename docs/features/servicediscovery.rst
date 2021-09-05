服务发现
===============

`nacos-sdk-csharp` 操作 nacos 的配置提供了两个 nuget 包，一个是原生的 sdk 版本， 一个是集成了 ASP.NET Core 配置体系的版本，大家可以根据自己的需要选择不同的版本。

::

    注: 请还在使用 nuget 包的名称里面还带有 `unofficial` 的朋友尽快升级到
    不带 `unofficial` 的版本，新版本同时支持 nacos server 1.x 和 2.x


原生的 sdk 版本
^^^^^^^^^^^^^^^^^^^

原生 sdk 暴露出了下面几个方法

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


可以看到有很多重载的方法，主要也是离不开服务的 CURD 和监听。


集成 ASP.NET Core 版本
^^^^^^^^^^^^^^^^^^^^^^^^

推出这样一个版本很大程度是为了简化操作，可以在应用启动的时候注册到 nacos 的注册中心，应用停止的时候可以注销。

SDK 内部是通过实现了一个后台服务 (IHostedService) 来达到这个效果的。

无论是注册还是注销，都会有重试的机制，目前最多重试 3 次。

查询服务实例则需要通过上面 **INacosNamingService** 提供的方法来实现。
