namespace Nacos.AspNetCore.Tests
{
    using System.Collections.Generic;
    using Xunit;

    public class WeightRandomLBStrategyTest
    {
        [Fact]
        public void GetHostTest_When_InstanceId_IsNotEmpty()
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

        [Fact]
        public void GetHostTest_When_InstanceId_AreTheSame()
        {
            WeightRandomLBStrategy strategy = new WeightRandomLBStrategy();
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
            WeightRandomLBStrategy strategy = new WeightRandomLBStrategy();
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
