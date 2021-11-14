using Nacos.AspNetCore.V2;

var builder = WebApplication.CreateBuilder(args);

// nacos server v1.x or v2.x
builder.Services.AddNacosAspNet(builder.Configuration);

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

app.Run("http://*:9876");
