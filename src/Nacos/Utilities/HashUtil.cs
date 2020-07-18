namespace Nacos.Utilities
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class HashUtil
    {
        public static string GetMd5(string value)
        {
            var result = string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            using (var md5 = MD5.Create())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
                var sBuilder = new StringBuilder();
                foreach (byte t in data)
                {
                    sBuilder.Append(t.ToString("x2"));
                }

                result = sBuilder.ToString();
            }

            return result;
        }

        public static string GetHMACSHA1(string value, string key)
        {
            byte[] secrectKey = Encoding.UTF8.GetBytes(key);
            using (HMACSHA1 hmac = new HMACSHA1(secrectKey))
            {
                hmac.Initialize();

                byte[] bytes_hmac_in = Encoding.UTF8.GetBytes(value);
                byte[] bytes_hamc_out = hmac.ComputeHash(bytes_hmac_in);

                string str_hamc_out = Convert.ToBase64String(bytes_hamc_out);

                return str_hamc_out;
            }
        }
    }
}
