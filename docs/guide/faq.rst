常见问题
===============

这个文档记录了一些 SDK 和 nacos server 对接使用上的常见问题。

1. 命名空间问题 
^^^^^^^^^^^^^^^^^^^^^^

命名空间可以说是第一要素，如果这个没有设置对，那么在控制台里面会看不到对应命名空间下面的数据。

在新建命名空间时，是可以指定命名空间Id的，不指定的话会自动生成一个UUID。

在SDK的配置里面，配置的一定是命名空间Id。如果是 public 命名空间，配置的是一个空字符串。


2. nacos server 端口开放问题
^^^^^^^^^^^^^^^^^^^^^^^^^^

SDK 在 v1.x 版本之后，就是默认用 grpc 的方式和 nacos server 对接，这个时候会出现下面几种情况。

- a. nacos server 是 1.x 版本，SDK 版本 >= 1.0
- b. nacos server 是 2.x 版本，基于 docker/k8s 部署，只暴露了默认的 8848 端口，没有暴露 grpc 的端口， SDK版本 >= 1.0

这个时候会出现  `Client not connected,current status: STARTING` 的错误

针对 a 的情况，需要把 xxxUseRpc 设置为 false。

针对 b 的情况，需要把 9848 暴露出来。

如果修改了默认端口或者是通过环境变量设置了偏移，自行调整对应端口，参考 https://nacos.io/zh-cn/docs/2.0.0-compatibility.html

3. nacos-sdk-csharp 版本与 nacos server 版本关系
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

nacos server 目前主要有 1.x 版本和 2.x 版本

nacos-sdk-csharp 有 0.x unofficial 版本 和 1.x 版本

nacos-sdk-csharp 0.x unofficial 版本 只能应用于 nacos server 1.x 版本

nacos-sdk-csharp 1.x 版本 可以同时应用于 nacos server 1.x 版本 和 2.x 版本
