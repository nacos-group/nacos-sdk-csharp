namespace Nacos
{
    using System.Collections.Generic;

    public class GetInstanceResult
    {
        /// <summary>
        /// user extended attributes
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// unique id of this instance
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// instance port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Service
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// instance health status
        /// </summary>
        public bool Healthy { get; set; }

        /// <summary>
        /// instance ip
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// cluster information of instance
        /// </summary>
        public string ClusterName { get; set; }

        /// <summary>
        /// instance weight
        /// </summary>
        public double Weight { get; set; }
    }
}
