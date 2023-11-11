// See https://aka.ms/new-console-template for more information
using App4;
using Nacos.Config;

Console.WriteLine("begin");

CustomConfigListen configListen = new CustomConfigListen();

var svc = new NacosConfigService(new Nacos.NacosSdkOptions
{
    ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" },
    EndPoint = "",
    Namespace = "cs",
    UserName = "nacos",
    Password = "nacos",

    // this sample will add the filter to encrypt the config with AES.
    // ConfigFilterAssemblies = new System.Collections.Generic.List<string> { "App4" },
    TLSConfig = null,

    // swich to use http or rpc
    ConfigUseRpc = true,
});

Console.WriteLine(@"
***********选择操作***********
1-获取配置；
2-删除配置；
3-发布配置；
4-监听配置；
5-移除监听；
0-Exit；
");

while (true)
{
    Console.Write("输入操作:");
    var o = Console.ReadLine();

    switch (o)
    {
        case "1":
            await Get().ConfigureAwait(false);
            break;
        case "2":
            await Delete().ConfigureAwait(false);
            break;
        case "3":
            await Publish().ConfigureAwait(false);
            break;
        case "4":
            await Listen().ConfigureAwait(false);
            break;
        case "5":
            await UnListen().ConfigureAwait(false);
            break;
        case "0":
            return;
        default:
            Console.Write("不支持该操作");
            break;
    }
}

async Task Get()
{
    Console.Write("输入DataId:");
    var d = Console.ReadLine();
    var res = await svc.GetConfig(d, "g", 3000).ConfigureAwait(false);
    Console.WriteLine(res ?? "empty config");
}

async Task Delete()
{
    Console.Write("输入DataId:");
    var d = Console.ReadLine();
    var res = await svc.RemoveConfig(d, "g").ConfigureAwait(false);
    Console.WriteLine("d ok " + res);
}

async Task Publish()
{
    Console.Write("输入DataId:");
    var d = Console.ReadLine();
    var res = await svc.PublishConfig(d, "g", new System.Random().Next(1, 9999999).ToString()).ConfigureAwait(false);
    Console.WriteLine("p ok " + res);
}

async Task Listen()
{
    Console.Write("输入DataId:");
    var d = Console.ReadLine();
    await svc.AddListener(d, "g", configListen).ConfigureAwait(false);
    Console.WriteLine("al ok ");
}

async Task UnListen()
{
    Console.Write("输入DataId:");
    var d = Console.ReadLine();
    await svc.RemoveListener(d, "g", configListen).ConfigureAwait(false);
    Console.WriteLine("rl ok ");
}
