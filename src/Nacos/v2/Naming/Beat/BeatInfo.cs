namespace Nacos.Naming.Beat
{
    using System.Collections.Generic;

    public class BeatInfo
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        public int port { get; set; }

        public string ip { get; set; }

        public double? weight { get; set; }

        public string serviceName { get; set; }

        public string cluster { get; set; }

        public Dictionary<string, string> metadata { get; set; } = new Dictionary<string, string>();

        public bool scheduled { get; set; }

        public int period { get; set; }

        public bool stopped { get; set; }

        public override string ToString()
        {
            return "BeatInfo{" + "port=" + port + ", ip='" + ip + '\'' + ", weight=" + weight + ", serviceName='" + serviceName + '\'' + ", cluster='" + cluster + '\'' + ", metadata=" + metadata + ", scheduled=" + scheduled + ", period=" + period + ", stopped=" + stopped + '}';
        }
    }
}
