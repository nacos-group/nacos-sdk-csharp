using Serilog;
using Serilog.Events;

var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: outputTemplate)
    .CreateLogger();

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MsConfigApp.AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddControllers();

builder.Host.ConfigureAppConfiguration((c, b) =>
{
    var config = b.Build();

    // read configuration from config files
    // default is json
    // b.AddNacosV2Configuration(config.GetSection("NacosConfig"));
    b.AddNacosV2Configuration(config.GetSection("NacosConfig"), logAction: x => x.AddSerilog(Log.Logger));

    // specify ini or yaml
    // b.AddNacosV2Configuration(config.GetSection("NacosConfig"), Nacos.IniParser.IniConfigurationStringParser.Instance);
    // b.AddNacosV2Configuration(config.GetSection("NacosConfig"), Nacos.YamlParser.YamlConfigurationStringParser.Instance);
})
.UseSerilog();

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

try
{
    Log.ForContext<Program>().Information("Application starting...");
    app.Run("http://*:8787");
}
catch (Exception ex)
{
    Log.ForContext<Program>().Fatal(ex, "Application start-up failed!!");
}
finally
{
    Log.CloseAndFlush();
}
