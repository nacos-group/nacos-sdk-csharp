using Nacos.AspNetCore.V2;

var builder = WebApplication.CreateBuilder(args);

// nacos server v1.x or v2.x
builder.Services.AddNacosAspNet(x =>
{
    x.ServerAddresses = new List<string> { "http://localhost:8848/" };
    x.EndPoint = "";
    x.Namespace = "cs";
    x.ServiceName = "App2";
    x.GroupName = "DEFAULT_GROUP";
    x.ClusterName = "DEFAULT";
    x.Ip = "";
    x.PreferredNetworks = "";
    x.Port = 0;
    x.Weight = 100;
    x.RegisterEnabled = true;
    x.InstanceEnabled = true;
    x.Ephemeral = true;
    x.Secure = false;

    // swich to use http or rpc
    x.NamingUseRpc = true;
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

app.Run("http://*:9877");
