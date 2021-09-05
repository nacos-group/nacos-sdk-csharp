配置中心
===============

`nacos-sdk-csharp` 操作 nacos 的配置提供了两个 nuget 包，一个是原生的 sdk 版本， 一个是集成了 ASP.NET Core 配置体系的版本，大家可以根据自己的需要选择不同的版本。

::

    注: 请还在使用 nuget 包的名称里面还带有 `unofficial` 的朋友尽快升级到
    不带 `unofficial` 的版本，新版本同时支持 nacos server 1.x 和 2.x


原生的 sdk 版本
^^^^^^^^^^^^^^^^^^^

原生 sdk 暴露出了下面几个方法

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

主要就是配置的 CURD 和监听。

监听可以让客户端实时获取 nacos 上面的最新配置，这个对实现配置的热加载/热更新是一个很重要的基石。

实现监听，需要自定义一个实现 `IListener` 的监听者。

.. code-block:: csharp

        public class CusConfigListen : Nacos.V2.IListener
        {
            public void ReceiveConfigInfo(string configInfo)
            {
                // 这里会有配置变更的回调，在这里处理配置变更之后的逻辑。
                System.Console.WriteLine("config cb cb cb " + configInfo);
            }
        }

添加监听和移除监听，要保证是同一个监听者对象，不然会造成监听一直存在，移除不了的情况。

对于重要配置被修改需要通知到部门群或者其他地方时，通过监听就可以实现一个 webhook 的功能了。


集成 ASP.NET Core 版本
^^^^^^^^^^^^^^^^^^^^^^^^

推出这样一个版本很大程度是为了简化操作，只加几个配置，代码层级的用法几乎零改动，类似于 spring cloud 那样。

SDK 这一块通过实现了自定义的 **ConfigurationProvider** 和 **IConfigurationSource** 来达到了这个效果。

对于配置的热加载/热更新， SDK 则是实现了一个默认的监听者，在配置变更之后进行了 `OnReload` 的操作。

使用上需要在 `Program` 里面进行 **ConfigureAppConfiguration** 的设置。

.. code-block:: csharp

        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
            {
                var c = builder.Build();
                builder.AddNacosV2Configuration(c.GetSection("NacosConfig"));
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })

::

    注: 请注意 `IOptions<T>`、`IOptionsSnapshot<T>` 和 `IOptionsMonitor<T>` 的区别
    避免出现配置变更了，应用读取不到最新配置的情况！！！
