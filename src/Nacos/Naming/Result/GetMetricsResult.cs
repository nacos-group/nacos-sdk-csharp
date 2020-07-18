namespace Nacos
{
    public class GetMetricsResult
    {
        public int ServiceCount { get; set; }

        public double Load { get; set; }

        public double Mem { get; set; }

        public int ResponsibleServiceCount { get; set; }

        public int InstanceCount { get; set; }

        public double Cpu { get; set; }

        public string Status { get; set; }

        public int ResponsibleInstanceCount { get; set; }
    }
}
