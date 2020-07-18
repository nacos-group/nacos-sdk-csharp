namespace Nacos
{
    public class ClusterServer
    {
        public string Ip { get; set; }

        public int ServePort { get; set; }

        public string Site { get; set; }

        public double Weight { get; set; }

        public double AdWeight { get; set; }

        public bool Alive { get; set; }

        public int LastRefTime { get; set; }

        public string LastRefTimeStr { get; set; }

        public string Key { get; set; }
    }
}
