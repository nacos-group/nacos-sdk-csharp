namespace Nacos.Microsoft.Extensions.Configuration.NacosLog
{
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;
    using System;

    public class NacosLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string name)
        {
            return _factory.CreateLogger(name);
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public void Dispose()
        {
            _factory?.Dispose();
        }

        public static NacosLoggerFactory GetInstance(Action<ILoggingBuilder> builder = null)
        {
            if (_loggingFactory != null) return _loggingFactory;

            _factory = GetLoggerFactory(builder);
            var obj = new NacosLoggerFactory();
            return _loggingFactory = obj;
        }

        private static ILoggerFactory _factory;
        private static NacosLoggerFactory _loggingFactory;

        private static ILoggerFactory GetLoggerFactory(Action<ILoggingBuilder> builder = null)
        {
            if (_factory != null) return _factory;

            var serviceCollection = new ServiceCollection();
            if (builder != null)
            {
                serviceCollection.AddLogging(builder);
            }
            else
            {
                serviceCollection.AddLogging(x => x.AddConsole());
            }

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            return loggerFactory;
        }
    }
}
