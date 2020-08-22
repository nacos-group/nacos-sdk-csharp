namespace Nacos.AspNetCore.Tests
{
    using System.Collections.Generic;
    using Xunit;

    public class WeightRandomLBStrategyTest
    {
        [Fact]
        public void GetHostTest()
        {
            WeightRandomLBStrategy strategy = new WeightRandomLBStrategy();
            var list = new List<Host>()
            {
                new Host { InstanceId = "instance-1", Weight = 3 },
                new Host { InstanceId = "instance-2", Weight = 2 },
            };

            HashSet<string> set = new HashSet<string>();

            for (int i = 0; i < 20; i++)
            {
                var host = strategy.GetHost(list);
                set.Add(host.InstanceId);
            }

            Assert.Equal(2, set.Count);
        }
    }
}
