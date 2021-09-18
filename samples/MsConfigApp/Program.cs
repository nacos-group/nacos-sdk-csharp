namespace MsConfigApp
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Nacos.Microsoft.Extensions.Configuration;
    using Serilog;
    using Serilog.Events;

    public class Program
    {
        public static void Main(string[] args)
        {
            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: outputTemplate)
                /*.WriteTo.File(
                    path: "logs/ApiTpl.log",
                    outputTemplate: outputTemplate,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 5,
                    encoding: System.Text.Encoding.UTF8)*/
                .CreateLogger();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            try
            {
                Log.ForContext<Program>().Information("Application starting...");
                CreateHostBuilder(args, Log.Logger).Build().Run();
            }
            catch (System.Exception ex)
            {
                Log.ForContext<Program>().Fatal(ex, "Application start-up failed!!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, Serilog.ILogger logger) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureAppConfiguration((context, builder) =>
                 {
                     var c = builder.Build();

                     // read configuration from config files
                     // default is json
                     // builder.AddNacosV2Configuration(c.GetSection("NacosConfig"));
                     builder.AddNacosV2Configuration(c.GetSection("NacosConfig"), logAction: x => x.AddSerilog(logger));

                     // specify ini or yaml
                     // builder.AddNacosV2Configuration(c.GetSection("NacosConfig"), Nacos.IniParser.IniConfigurationStringParser.Instance);
                     // builder.AddNacosV2Configuration(c.GetSection("NacosConfig"), Nacos.YamlParser.YamlConfigurationStringParser.Instance);

                     // hard code here
                     /*builder.AddNacosV2Configuration(x =>
                     {
                         x.Namespace = "cs";
                         x.ServerAddresses = new List<string> { "http://localhost:8848" };
                         x.Listeners = new List<ConfigListener>
                         {
                             new ConfigListener { DataId = "d1", Group = "g", Optional = false },
                         };
                     });*/
                 })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://*:8787");
                })
                .UseSerilog();
    }
}
