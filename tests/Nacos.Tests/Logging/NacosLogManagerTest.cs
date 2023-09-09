namespace Nacos.Tests.Logging
{
    using Microsoft.Extensions.Logging;
    using Nacos.Logging;
    using Serilog;
    using System;
    using System.IO;
    using Xunit;
    using MsILogger = Microsoft.Extensions.Logging.ILogger;

    public class NacosLogManagerTest
    {
        private readonly string _logFilePath = "serilog.log";

        public NacosLogManagerTest()
        {
        }

        [Fact]
        public void Logger_For_Ms_Console()
        {
            MsILogger logger1;
            MsILogger logger2;

            // Base on microsoft default logger
            var msLoggerFactory = LoggerFactory.Create(
                builder => builder
                   .AddFilter("logger", LogLevel.Information)
                   .AddConsole());
            NacosLogManager.UseLoggerFactory(msLoggerFactory);
            logger1 = NacosLogManager.CreateLogger<NacosLogManagerTest>();
            logger2 = NacosLogManager.CreateLogger(typeof(NacosLogManagerTest) + "_Name");

            logger1.LogTrace("This is a trace message.");
            logger1.LogDebug("This is a debug message.");
            logger1.LogInformation("This is an info message.");
            logger1.LogWarning("This is a warn message.");
            logger1.LogError("This is an error message.");
            logger1.LogCritical("This is a critical message.");

            logger1.LogError(new Exception("exception msg"), "this is an error message with exception.");
            logger1.LogCritical(new Exception("exception msg"), "this is a critical message with exception.");

            logger2.LogTrace("This is a trace message.");
            logger2.LogDebug("This is a debug message.");
            logger2.LogInformation("This is an info message.");
            logger2.LogWarning("This is a warn message.");
            logger2.LogError("This is an error message.");
            logger2.LogCritical("This is a critical message.");

            logger2.LogError(new Exception("exception msg"), "this is an error message with exception.");
            logger2.LogCritical(new Exception("exception msg"), "this is a critical message with exception.");
        }

        [Fact]
        public void Logger_For_Serilog_File()
        {
            MsILogger logger1;
            MsILogger logger2;

            // Delete serilog log file
            if (File.Exists(_logFilePath))
            {
                File.Delete(_logFilePath);
            }

            // Base on serilog logger
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.File(_logFilePath, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] <{SourceContext:l}> {Message}{NewLine}{Exception}")
                .CreateLogger();
            var slLoggerFactory = new LoggerFactory().AddSerilog(Log.Logger);
            NacosLogManager.UseLoggerFactory(slLoggerFactory);
            logger1 = NacosLogManager.CreateLogger<NacosLogManagerTest>();
            logger2 = NacosLogManager.CreateLogger(typeof(NacosLogManagerTest) + "_Name");

            logger1.LogTrace("This is a trace message.");
            logger1.LogDebug("This is a debug message.");
            logger1.LogInformation("This is an info message.");
            logger1.LogWarning("This is a warn message.");
            logger1.LogError("This is an error message.");
            logger1.LogCritical("This is a critical message.");

            logger1.LogError(new Exception("exception msg"), "this is an error message with exception.");
            logger1.LogCritical(new Exception("exception msg"), "this is a critical message with exception.");

            logger2.LogTrace("This is a trace message.");
            logger2.LogDebug("This is a debug message.");
            logger2.LogInformation("This is an info message.");
            logger2.LogWarning("This is a warn message.");
            logger2.LogError("This is an error message.");
            logger2.LogCritical("This is a critical message.");

            logger2.LogError(new Exception("exception msg"), "this is an error message with exception.");
            logger2.LogCritical(new Exception("exception msg"), "this is a critical message with exception.");

            Assert.True(File.Exists(_logFilePath));
        }
    }
}
