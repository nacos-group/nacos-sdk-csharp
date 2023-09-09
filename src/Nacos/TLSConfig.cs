namespace Nacos
{
    public class TLSConfig
    {
        /// <summary>
        /// whether enable tls
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// pfx file path
        /// </summary>
        public string PfxFile { get; set; }

        /// <summary>
        /// password of pfx
        /// </summary>
        public string Password { get; set; }
    }
}
