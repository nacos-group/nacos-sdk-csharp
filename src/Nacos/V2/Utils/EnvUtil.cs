namespace Nacos.V2.Utils
{
    using System;

    public static class EnvUtil
    {
        public static string GetEnvValue(string envName)
        {
            var value = Environment.GetEnvironmentVariable(envName);
            return value;
        }

        public static string GetEnvValue(string envName, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(envName);

            return value.IsNullOrWhiteSpace() ? defaultValue : value;
        }
    }
}
