# MsConfigApp

This sample shows how to integrate ASP.NET Core Configuration System.


## Preparations

### 1. Running up nacos server

https://nacos.io/en-us/docs/quick-start.html

https://nacos.io/en-us/docs/quick-start-docker.html

### 2. Modifying `appsettings.json` to replace your configuration

```JSON
{
  "NacosConfig": {
    "Listeners": [
      {
        "Optional": false,
        "DataId": "common",
        "Group": ""
      },
      {
        "Optional": false,
        "DataId": "demo",
        "Group": ""
      }
    ],
    "Optional": false, // after v0.8.0,  Obsolete, use Listeners to configure
    "DataId": "demo", // after v0.8.0,  Obsolete, use Listeners to configure
    "Group": "group1", // after v0.8.0,  Obsolete, use Listeners to configure
    "Tenant": "9a760099-7724-4505-bb3d-e80028d53b35",
    "ServerAddresses": [ "http://localhost:8848/" ],
    "UserName": "test2",
    "Password": "123456",
    "AccessKey": "",
    "SecretKey": "",
    "EndPoint": "acm.aliyun.com",
    "ConfigFilterAssemblies": [ "MsConfigApp" ],
    "ConfigFilterExtInfo": "{\"JsonPaths\":[\"ConnectionStrings.Default\"],\"Other\":\"xxxxxx\"}"
  }
}
```

> NOTE: after v0.8.0, we support listen multi-config. If you configure `Listeners`, the `Optional`, `DataId` and `Group` in the root section will not take effect. If no `Listeners` was configured, they will take effect.

### 3. Create configuration in nacos server

Create configurations whoes dataid and group are the same as `appsettings.json`

In this sample, you should create two configurations. 

The first one, its dataid is demo, group is DFAULT_GROUP and the value is as following.

```JSON
{
    "ConnectionStrings": {
        "Default": "Server=127.0.0.1;Port=3306;Database=demo;User Id=root;Password=123456;"
    },
    "version": "测试version",
    "AppSettings": {
        "Str": "val",
        "num": 1,
        "arr": [1, 2, 3],
        "subobj": {
            "a": "b"
        }
    }
}
```

> NOTE: If **ConfigFilterAssemblies** is not empty, using the following config value.


```JSON
{
    "ConnectionStrings": {
        "Default": "U2VydmVyPTEyNy4wLjAuMTtQb3J0PTMzMDY7RGF0YWJhc2U9ZGVtbztVc2VyIElkPXJvb3Q7UGFzc3dvcmQ9MTIzNDU2Ow=="
    },
    "version": "测试version",
    "AppSettings": {
        "Str": "val",
        "num": 1,
        "arr": [1, 2, 3],
        "subobj": {
            "a": "b"
        }
    }
}
```

The second one, its dataid is common, group is DFAULT_GROUP and the value is as following.

```JSON
{
    "all": "test"
}
```
