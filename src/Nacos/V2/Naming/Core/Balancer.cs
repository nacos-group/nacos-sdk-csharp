namespace Nacos.V2.Naming.Core
{
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Utils;
    using System.Collections.Generic;
    using System.Linq;

    public class Balancer
    {
        private static readonly string _uniqueKey = "nacos-sdk-csharp";

        public static Instance GetHostByRandomWeight(List<Instance> hosts)
        {
            if (hosts == null || !hosts.Any()) return null;

            List<Pair<Instance>> hostsWithWeight = new List<Pair<Instance>>();
            foreach (var host in hosts)
            {
                if (host.Healthy)
                {
                    hostsWithWeight.Add(new Pair<Instance>(host, host.Weight));
                }
            }

            Chooser<string, Instance> vipChooser = new Chooser<string, Instance>(_uniqueKey);
            vipChooser.Refresh(hostsWithWeight);
            return vipChooser.RandomWithWeight();
        }

        public static Instance GetHostByRandom(List<Instance> hosts)
        {
            if (hosts == null || !hosts.Any()) return null;

            List<Pair<Instance>> hostsWithWeight = new List<Pair<Instance>>();
            foreach (var host in hosts)
            {
                if (host.Healthy)
                {
                    hostsWithWeight.Add(new Pair<Instance>(host, host.Weight));
                }
            }

            Chooser<string, Instance> vipChooser = new Chooser<string, Instance>(_uniqueKey);
            return vipChooser.Random();
        }
    }
}