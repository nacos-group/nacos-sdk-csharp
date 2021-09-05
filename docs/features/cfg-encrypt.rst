配置加解密
===============

配置加解密这个功能是衍生功能，目前和 nacos 服务端联系还不大，是 SDK 对配置加密后再传输，解密后再使用。

如果需要使用这个功能，需要做下面两件事。

1. 自定义 ConfigFilter
2. 配置 ConfigFilter 


自定义 ConfigFilter
^^^^^^^^^^^^^^^^^^^^^^

把配置加解密的逻辑放到自定义的 ConfigFilter 里面。 ConfigFilter 可以有多个，不同 ConfigFilter 的执行顺序是按他们的 Order 属性来决定的。

.. code-block:: csharp

    public class MyNacosConfigFilter : IConfigFilter
    {
        public void DoFilter(IConfigRequest request, IConfigResponse response, IConfigFilterChain filterChain)
        {
            if (request != null)
            {
                // 这里是请求的过滤，也就是在这里进行加密操作

                
                // 不要忘了在这里覆盖请求的内容！！！！！
                request.PutParameter(Nacos.V2.Config.ConfigConstants.ENCRYPTED_DATA_KEY, encryptedDataKey);
                request.PutParameter(Nacos.V2.Config.ConfigConstants.CONTENT, content);
            }

            if (response != null)
            {
                // 这里是响应的过滤，也就是在这里进行解密操作

                
                // 不要忘了在这里覆盖响应的内容！！！！！
                response.PutParameter(Nacos.V2.Config.ConfigConstants.CONTENT, content);
            }
        }

        public string GetFilterName() => nameof(MyNacosConfigFilter);

        public int GetOrder() => 1;

        public void Init(NacosSdkOptions options)
        {
            // 做一些初始化操作
        }        
    }


配置 ConfigFilter 
^^^^^^^^^^^^^^^^^^^^^^

这一步主要是告诉 nacos-sdk-csharp 去那个程序集找 ConfigFilter 的实现，以及它的实现需要什么参数。

.. code-block:: JSON

    {
        "NacosConfig": {           
            "ConfigFilterAssemblies": [ "XXXX.CusLib" ],
            "ConfigFilterExtInfo": "{\"JsonPaths\":[\"ConnectionStrings.Default\"],\"Other\":\"xxxxxx\"}"
        }
    }


这里主要是两个配置，一个是 `ConfigFilterAssemblies` 指定实现类所在的程序集，一个是 `ConfigFilterExtInfo` 指定实现类可能需要的参数。
