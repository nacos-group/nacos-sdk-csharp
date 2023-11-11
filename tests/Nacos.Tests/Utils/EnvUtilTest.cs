namespace Nacos.Tests.Utils
{
    using Nacos.Utils;
    using System;
    using Xunit;

    public class EnvUtilTest
    {
        [Fact]
        public void GetEnvValue_Should_Succeed()
        {
            var envName = "nacos.test.env.name";
            var envValue = "nacos.test.env.value";
            Environment.SetEnvironmentVariable(envName, envValue);

            var getEnvValue = EnvUtil.GetEnvValue(envName);
            Assert.Equal(envValue, getEnvValue);

            getEnvValue = EnvUtil.GetEnvValue(envName + ".temp", "nacos.test.env.value.default");
            Assert.Equal(envValue + ".default", getEnvValue);
        }
    }
}
