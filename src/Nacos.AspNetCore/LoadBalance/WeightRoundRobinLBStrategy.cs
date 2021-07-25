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
            // aliyun sae, the instanceid returns empty string
            // when the instanceid is empty, create a new one, but the group was missed.
            list.ForEach(x => { x.InstanceId = string.IsNullOrWhiteSpace(x.InstanceId) ? $"{x.Ip}#{x.Port}#{x.ClusterName}#{x.ServiceName}" : x.InstanceId; });

            var tmp = list.Select(x => new LbKv
            {
                InstanceId = x.InstanceId,
                Weight = x.Weight
            }).GroupBy(x => x.InstanceId).Select(x => new LbKv
            {
                InstanceId = x.Key,
                Weight = x.Max(y => y.Weight)
            }).ToList();

            // <instanceid, weight>
            var dic = tmp.OrderByDescending(o => o.Weight).ToDictionary(k => k.InstanceId, v => v.Weight);

            var srcInstanceIdList = dic.Keys.ToList();
            var tagInstanceIdList = new List<string>();

            var minWeight = dic.Values.Min();

            foreach (var item in srcInstanceIdList)
            {
                dic.TryGetValue(item, out var weight);

                var count = Math.Round(weight / minWeight);
                for (int i = 0; i < count; i++)
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

            var instance = list.FirstOrDefault(x => x.InstanceId.Equals(instanceId));

            if (instance == null)
            {
                var arr = instanceId.Split("#");
                var ip = arr[0];
                int.TryParse(arr[1], out var port);
                var cluster = arr[2];

                instance = list.First(x => x.Ip.Equals(ip) && x.Port == port && x.ClusterName.Equals(cluster));
            }

            return instance;
        }
    }
}
