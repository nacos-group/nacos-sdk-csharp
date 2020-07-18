namespace Nacos.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WeightRandomLBStrategy : ILBStrategy
    {
        public LBStrategyName Name => LBStrategyName.WeightRandom;

        public string GetInstance(List<NacosServer> list)
        {
            var dict = BuildScore(list);

            var instance = string.Empty;

            var rd = new Random().NextDouble();

            foreach (var item in dict)
            {
                if (item.Value >= rd)
                {
                    instance = item.Key;
                    break;
                }
            }

            return instance;
        }

        private Dictionary<string, double> BuildScore(List<NacosServer> list)
        {
            var dict = new Dictionary<string, double>();
            var total = list.Sum(x => x.Weight);
            var cur = 0d;

            foreach (var item in list)
            {
                cur += item.Weight;
                dict.Add(item.Url, cur / total);
            }

            return dict;
        }
    }
}
