namespace Nacos
{
    public class Host
    {
        public bool Valid { get; set; }

        public bool Marked { get; set; }

        public string InstanceId { get; set; }

        public int Port { get; set; }

        public string Ip { get; set; }

        public double Weight { get; set; }

        public object Metadata { get; set; }
    }
}
