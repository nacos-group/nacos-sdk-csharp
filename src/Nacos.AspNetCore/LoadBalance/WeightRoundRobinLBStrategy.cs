namespace Nacos.AspNetCore
{
    using Nacos;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class WeightRoundRobinLBStrategy : ILBStrategy
    {
        public LBStrategyName Name => LBStrategyName.WeightRoundRobin;

        private int _pos;

        private static object obj = new object();

        public Host GetHost(List<Host> list)
        {
            // <instanceid, weight>
            var dic = list.ToDictionary(k => k.InstanceId, v => (int)v.Weight);

            var srcInstanceIdList = dic.Keys.ToList();
            var tagInstanceIdList = new List<string>();

            foreach (var item in srcInstanceIdList)
            {
                dic.TryGetValue(item, out var weight);

                for (int i = 0; i < weight; i++)
                    tagInstanceIdList.Add(item);
            }

            var instanceId = string.Empty;

            lock (obj)
            {
                if (_pos >= tagInstanceIdList.Count)
                    _pos = 0;

                instanceId = tagInstanceIdList[_pos];
                _pos++;
            }

            return list.First(x => x.InstanceId.Equals(instanceId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
