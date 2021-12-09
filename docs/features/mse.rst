集成微服务引擎MSE
===================

前置条件
^^^^^^^^^^^^^^^^^^^

注册阿里云账号，并开通和购买微服务引擎。


使用 SDK 操作的话，只需要将 ServerAddresses 替换成 mse 实例对应的地址即可。

示例如下：

配置模块
^^^^^^^^^^^^^^^^^^^^^^^^

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
            "Namespace": "0138xxxx-yyyy-zzzz-1111-000000000000",
            "ServerAddresses": [ "http://mse-xxxxxxxxxxx-nacos-ans.mse.aliyuncs.com:8848" ]
        }
    }


服务注册发现模块
^^^^^^^^^^^^^^^^^^^^^^^^

.. code-block:: json

    {
        "nacos": {
            "ServerAddresses": [ "http://mse-xxxxxxxxxxx-nacos-ans.mse.aliyuncs.com:8848" ],
            "DefaultTimeOut": 15000,
            "Namespace": "0138xxxx-yyyy-zzzz-1111-000000000000",
            "ListenInterval": 1000,
            "ServiceName": "App1",
            "GroupName": "DEFAULT_GROUP",
            "ClusterName": "DEFAULT",            
            "Weight": 100,
            "RegisterEnabled": true,
            "InstanceEnabled": true,
            "Ephemeral": true           
        }
    }

