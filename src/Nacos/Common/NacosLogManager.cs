namespace Nacos.Common
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    public static class NacosLogManager
    {
        private static ILoggerFactory _loggerFactory;

        public static ILogger<T> CreateLogger<T>()
        {
            return _loggerFactory?.CreateLogger<T>() ?? NullLogger<T>.Instance;
        }

        public static ILogger CreateLogger(string category)
        {
            return _loggerFactory?.CreateLogger(category) ?? NullLogger.Instance;
        }

        public static void UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new System.ArgumentNullException(nameof(loggerFactory));
        }
    }
}
