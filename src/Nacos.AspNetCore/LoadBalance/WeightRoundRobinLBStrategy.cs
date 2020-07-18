namespace Nacos.AspNetCore
{
    using System.Collections.Generic;
    using System.Threading;

    public class WeightRoundRobinLBStrategy : ILBStrategy
    {
        public LBStrategyName Name => LBStrategyName.WeightRoundRobin;

        private int _count;

        public string GetInstance(List<NacosServer> list)
        {
            var listStr = new List<string>();

            foreach (var item in list)
            {
                for (int i = 0; i < (int)item.Weight; i++)
                {
                    listStr.Add(item.Url);
                }
            }

            var len = list.Count;

            var instance = list[_count % len];

            Interlocked.Increment(ref _count);

            // Interlocked.Exchange(ref _count, Interlocked.Increment(ref _count) % len);
            return instance.Url;
        }
    }
}
