namespace Nacos.Microsoft.Extensions.Configuration
{
    public class ConfigListener
    {
        /// <summary>
        /// Determines if the Nacos Server is optional
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        public string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        public string Tenant { get; set; }
    }
}
