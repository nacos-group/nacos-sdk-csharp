namespace Nacos
{
    using System.Collections.Generic;

    public class ListInstancesResult
    {
        public string Dom { get; set; }

        public string Name { get; set; }

        public int CacheMillis { get; set; }

        public string UseSpecifiedURL { get; set; }

        public List<Host> Hosts { get; set; }

        public string Checksum { get; set; }

        public long LastRefTime { get; set; }

        public string Env { get; set; }

        public string Clusters { get; set; }
    }
}
