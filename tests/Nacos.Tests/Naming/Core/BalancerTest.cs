namespace Nacos.Tests.Naming.Core
{
    using Nacos.Naming.Core;
    using Nacos.Naming.Dtos;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class BalancerTest
    {
        private List<Instance> instances = new List<Instance>();
        private int healthyIndex = 0;

        public BalancerTest(ITestOutputHelper testOutput)
        {
            var count = 10;
            healthyIndex = new Random().Next(count);
            testOutput.WriteLine(healthyIndex.ToString());
            for (int i = 1; i < count + 1; i++)
            {
                var instance = new Instance
                {
                    InstanceId = Guid.NewGuid().ToString("N"),
                    Ip = "192.168.0." + i,
                    Port = 1010 + i,
                    Weight = i,
                    Healthy = i == healthyIndex + 1,
                    ClusterName = "DEFAULT",
                    ServiceName = "DEFAULT-" + i,
                    Metadata = new Dictionary<string, string>
                    {
                        { "aa", "bb" },
                        { "cc", "dd" }
                    }
                };
                instances.Add(instance);
                testOutput.WriteLine(instance.ToString());
            }
        }

        [Fact]
        public void GetHostByRandomWeight_Should_Successed()
        {
            var instance = Balancer.GetHostByRandomWeight(instances);
            Assert.NotNull(instance);
            Assert.Equal(instances[healthyIndex], instance);
        }

        [Fact]
        public void GetHostByRandom_Should_Successed()
        {
            var instance = Balancer.GetHostByRandom(instances);
            Assert.NotNull(instance);
            Assert.Equal(instances[healthyIndex], instance);
        }
    }
}
