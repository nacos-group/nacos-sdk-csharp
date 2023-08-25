namespace Nacos.Auth.Ram.Utils
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    internal static class SignUtil
    {
        internal static string Sign(string value, string key)
        {
            byte[] secrectKey = Encoding.UTF8.GetBytes(key);
            using HMACSHA1 hmac = new(secrectKey);
            hmac.Initialize();

            byte[] bytes_hmac_in = Encoding.UTF8.GetBytes(value);
            byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);

            string str_hamc_out = Convert.ToBase64String(bytes_hamc_out);

            return str_hamc_out;
        }
    }
}
