using Nacos.V2.DependencyInjection;
using Nacos.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNacosV2Config(x =>
{
    x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
    x.EndPoint = "";
    x.Namespace = "cs";
    x.UserName = "nacos";
    x.Password = "nacos";

    // this sample will add the filter to encrypt the config with AES.
    x.ConfigFilterAssemblies = new System.Collections.Generic.List<string> { "App3" };

    // swich to use http or rpc
    x.ConfigUseRpc = true;
});

builder.Services.AddNacosV2Naming(x =>
{
    x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
    x.EndPoint = "";
    x.Namespace = "cs";

    // swich to use http or rpc
    x.NamingUseRpc = true;
});

builder.Services.AddNacosOpenApi(x =>
{
    x.ServerAddresses = new System.Collections.Generic.List<string> { "http://localhost:8848/" };
    x.EndPoint = "";
    x.Namespace = "cs";
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run("http://*:9632");
