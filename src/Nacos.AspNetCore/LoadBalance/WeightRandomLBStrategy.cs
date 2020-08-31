namespace Nacos.AspNetCore
{
    using Nacos;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WeightRandomLBStrategy : ILBStrategy
    {
        public LBStrategyName Name => LBStrategyName.WeightRandom;

        public Host GetHost(List<Host> list)
        {
            var dict = BuildScore(list);

            Host instance = null;

            var rd = new Random().NextDouble();

            foreach (var item in dict)
            {
                if (item.Value >= rd)
                {
                    instance = list.FirstOrDefault(x => x.InstanceId.Equals(item.Key));

                    if (instance == null)
                    {
                        var arr = item.Key.Split("#");
                        var ip = arr[0];
                        int.TryParse(arr[1], out var port);
                        var cluster = arr[2];

                        instance = list.First(x => x.Ip.Equals(ip) && x.Port == port && x.ClusterName.Equals(cluster));
                    }

                    break;
                }
            }

            return instance;
        }

        private Dictionary<string, double> BuildScore(List<Host> list)
        {
            var dict = new Dictionary<string, double>();

            var tmp = list.Select(x => new LbKv
            {
                // aliyun sae, the instanceid returns empty string
                // when the instanceid is empty, create a new one, but the group was missed.
                InstanceId = string.IsNullOrWhiteSpace(x.InstanceId) ? $"{x.Ip}#{x.Port}#{x.ClusterName}#{x.ServiceName}" : x.InstanceId,
                Weight = x.Weight
            }).GroupBy(x => x.InstanceId).Select(x => new LbKv
            {
                InstanceId = x.Key,
                Weight = x.Max(y => y.Weight)
            }).ToList();

            var total = tmp.Sum(x => x.Weight);
            var cur = 0d;

            foreach (var item in tmp)
            {
                cur += item.Weight;
                dict.TryAdd(item.InstanceId, cur / total);
            }

            return dict;
        }
    }
}
