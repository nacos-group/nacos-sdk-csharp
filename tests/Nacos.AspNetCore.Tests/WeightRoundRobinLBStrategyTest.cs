namespace Nacos.AspNetCore.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class WeightRoundRobinLBStrategyTest
    {
        [Fact]
        public void GetHostTest()
        {
            WeightRoundRobinLBStrategy strategy = new WeightRoundRobinLBStrategy();
            var list = new List<Host>()
            {
                new Host { InstanceId = "instance-1", Weight = 1 },
                new Host { InstanceId = "instance-2", Weight = 2 },
                new Host { InstanceId = "instance-3", Weight = 1 },
            };

            List<string> res = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                var host = strategy.GetHost(list);
                res.Add(host.InstanceId);
            }

            var i1 = res.Count(x => x.Equals("instance-1"));
            var i2 = res.Count(x => x.Equals("instance-2"));
            var i3 = res.Count(x => x.Equals("instance-3"));

            Assert.Equal(1, i1);
            Assert.Equal(2, i2);
            Assert.Equal(1, i3);
        }
    }
}
