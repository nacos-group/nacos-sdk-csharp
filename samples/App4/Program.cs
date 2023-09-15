// See https://aka.ms/new-console-template for more information
using Nacos.Config;

Console.WriteLine("begin");

var svc = new NacosConfigService(new Nacos.NacosSdkOptions
{
    ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" },
    EndPoint = "",
    Namespace = "cs",
    UserName = "nacos",
    Password = "nacos",

    // this sample will add the filter to encrypt the config with AES.
    ConfigFilterAssemblies = new System.Collections.Generic.List<string> { "App4" },
    TLSConfig = null,

    // swich to use http or rpc
    ConfigUseRpc = true,
});

Console.WriteLine(@"
***********选择操作***********
1-获取配置；
2-删除配置；
3-发布配置；
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
