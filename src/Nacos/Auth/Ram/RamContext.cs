namespace Nacos.Auth.Ram
{
    using Nacos.Utils;

    public class RamContext
    {
        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public string RamRoleName { get; set; }

        public bool Validate()
        {
            return RamRoleName.IsNotNullOrWhiteSpace() ||
                (AccessKey.IsNotNullOrWhiteSpace() && SecretKey.IsNotNullOrWhiteSpace());
        }
    }
}
