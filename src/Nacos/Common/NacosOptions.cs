namespace Nacos
{
    using System.Collections.Generic;

    public class NacosOptions
    {
        /// <summary>
        /// nacos server addresses.
        /// </summary>
        /// <example>
        /// http://10.1.12.123:8848,https://10.1.12.124:8848
        /// </example>
        public List<string> ServerAddresses { get; set; }

        /// <summary>
        /// EndPoint
        /// </summary>
        public string EndPoint { get; set; }

        public string ContextPath { get; set; } = "nacos";

        public string ClusterName { get; set; } = "serverlist";

        /// <summary>
        /// default timeout, unit is Milliseconds.
        /// </summary>
        public int DefaultTimeOut { get; set; } = 15000;

        /// <summary>
        /// default namespace
        /// </summary>
        public string Namespace { get; set; } = "";

        /// <summary>
        /// accessKey
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// secretKey
        /// </summary>
        public string SecretKey { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// listen interval, unit is millisecond.
        /// </summary>
        public int ListenInterval { get; set; } = 1000;
    }
}
