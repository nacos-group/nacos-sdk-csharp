namespace Nacos
{
    using System.Collections.Generic;

    public class Cluster
    {
        /// <summary>
        /// Health check config of this cluster
        /// </summary>
        public HealthChecker HealthChecker { get; set; }

        /// <summary>
        /// Metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Name of cluster
        /// </summary>
        public string Name { get; set; }
    }
}
