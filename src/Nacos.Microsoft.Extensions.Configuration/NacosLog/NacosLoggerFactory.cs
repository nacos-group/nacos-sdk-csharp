namespace Nacos.Microsoft.Extensions.Configuration.NacosLog
{
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Logging;

    public class NacosLoggerFactory : ILoggerFactory
    {
        public NacosLoggerFactory()
        {
        }

        static NacosLoggerFactory()
        {
            _factory = GetLoggerFactory();
        }

        public static readonly NacosLoggerFactory Instance = new NacosLoggerFactory();

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

        private static ILoggerFactory _factory;

        private static ILoggerFactory GetLoggerFactory()
        {
            if (_factory != null) return _factory;

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(x => x.AddConsole());
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            return loggerFactory;
        }
    }
}
