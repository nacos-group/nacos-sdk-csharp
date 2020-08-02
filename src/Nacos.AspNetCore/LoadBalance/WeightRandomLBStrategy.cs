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
                    instance = list.First(x => x.InstanceId.Equals(item.Key));
                    break;
                }
            }

            return instance;
        }

        private Dictionary<string, double> BuildScore(List<Host> list)
        {
            var dict = new Dictionary<string, double>();
            var total = list.Sum(x => x.Weight);
            var cur = 0d;

            foreach (var item in list)
            {
                cur += item.Weight;
                dict.Add(item.InstanceId, cur / total);
            }

            return dict;
        }
    }
}
