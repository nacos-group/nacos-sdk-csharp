namespace Nacos.V2
{
    using System.Collections.Generic;

    public class NacosSdkOptions
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

        public string RamRoleName { get; set; }

        /// <summary>
        /// listen interval, unit is millisecond.
        /// </summary>
        public int ListenInterval { get; set; } = 1000;

        public bool ConfigUseRpc { get; set; } = true;

        public bool NamingUseRpc { get; set; } = true;

        public string NamingLoadCacheAtStart { get; set; }

        public string NamingCacheRegistryDir { get; set; }

        /// <summary>
        /// Whether enable protecting naming push empty data, default is false.
        /// </summary>
        public bool NamingPushEmptyProtection { get; set; } = false;

        /// <summary>
        /// Specify the assemblies that contains the impl of IConfigFilter.
        /// </summary>
        public List<string> ConfigFilterAssemblies { get; set; }

        /// <summary>
        /// Specify some extension info of IConfigFilter.
        /// </summary>
        public string ConfigFilterExtInfo { get; set; }
    }
}
