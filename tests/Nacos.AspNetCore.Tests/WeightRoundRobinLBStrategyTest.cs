namespace Nacos.AspNetCore.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    [Trait("Category", "all")]
    public class WeightRoundRobinLBStrategyTest
    {
        [Fact]
        public void GetHostTest_When_InstanceId_IsNotEmpty()
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

        [Fact]
        public void GetHostTest_When_InstanceId_AreTheSame()
        {
            WeightRoundRobinLBStrategy strategy = new WeightRoundRobinLBStrategy();
            var list = new List<Host>()
            {
                new Host { InstanceId = "instance-1", Weight = 3 },
                new Host { InstanceId = "instance-2", Weight = 2 },
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

        [Fact]
        public void GetHostTest_When_InstanceId_HasEmpty()
        {
            WeightRoundRobinLBStrategy strategy = new WeightRoundRobinLBStrategy();
            var list = new List<Host>()
            {
                new Host { InstanceId = "instance-1", Weight = 3, Ip = "127.0.0.2", Port = 80, ClusterName = "s", ServiceName = "ss" },
                new Host { InstanceId = "", Weight = 2, Ip = "127.0.0.1", Port = 80, ClusterName = "s", ServiceName = "ss" },
                new Host { InstanceId = null, Weight = 2, Ip = "127.0.0.1", Port = 8080, ClusterName = "s", ServiceName = "ss" },
            };

            HashSet<string> set = new HashSet<string>();

            for (int i = 0; i < 20; i++)
            {
                var host = strategy.GetHost(list);
                set.Add(string.IsNullOrWhiteSpace(host.InstanceId) ? $"{host.Ip}#{host.Port}#{host.ClusterName}#{host.ServiceName}" : host.InstanceId);
            }

            Assert.Equal(3, set.Count);
        }
    }
}
