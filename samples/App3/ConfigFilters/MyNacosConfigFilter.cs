namespace App3.ConfigFilters
{
    using Nacos.V2;
    using Nacos.V2.Config.Abst;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class MyNacosConfigFilter : IConfigFilter
    {
        private static readonly string DefaultKey = "catcherwong00000";

        public void DoFilter(IConfigRequest request, IConfigResponse response, IConfigFilterChain filterChain)
        {
            if (request != null)
            {
                var encryptedDataKey = DefaultKey;
                var raw_content = request.GetParameter(Nacos.V2.Config.ConfigConstants.CONTENT);
                var content = AESEncrypt((string)raw_content, encryptedDataKey);

                // after encrypt the content, don't forget to update the request!!!
                request.PutParameter(Nacos.V2.Config.ConfigConstants.ENCRYPTED_DATA_KEY, encryptedDataKey);
                request.PutParameter(Nacos.V2.Config.ConfigConstants.CONTENT, content);
            }

            if (response != null)
            {
                var resp_content = response.GetParameter(Nacos.V2.Config.ConfigConstants.CONTENT);
                var resp_encryptedDataKey = response.GetParameter(Nacos.V2.Config.ConfigConstants.ENCRYPTED_DATA_KEY);

                // nacos 2.0.2 still do not return the encryptedDataKey yet
                // but we can use a const key here.
                // after nacos server return the encryptedDataKey, we can keep one dataid with one encryptedDataKey
                var encryptedDataKey = (resp_encryptedDataKey == null || string.IsNullOrWhiteSpace((string)resp_encryptedDataKey)) ? DefaultKey : (string)resp_encryptedDataKey;
                var content = AESDecrypt((string)resp_content, encryptedDataKey);
                response.PutParameter(Nacos.V2.Config.ConfigConstants.CONTENT, content);
            }
        }

        public string GetFilterName() => nameof(MyNacosConfigFilter);

        public int GetOrder() => 1;

        public void Init(NacosSdkOptions options)
        {
            Console.WriteLine("Assemblies = " + string.Join(",", options.ConfigFilterAssemblies));
            Console.WriteLine("Ext Info = " + string.Join(",", options.ConfigFilterExtInfo));
            Console.WriteLine("Init");
        }

        public static string? AESEncrypt(string data, string key)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(data);
                    byte[] bKey = new byte[32];
                    Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.KeySize = 256;
                    aes.Key = bKey;

                    using (CryptoStream cryptoStream = new CryptoStream(memory, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        try
                        {
                            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            return Convert.ToBase64String(memory.ToArray());
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public static string? AESDecrypt(string data, string key)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            byte[] encryptedBytes = Convert.FromBase64String(data);
            byte[] bKey = new byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(bKey.Length)), bKey, bKey.Length);

            using (MemoryStream memory = new MemoryStream(encryptedBytes))
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.KeySize = 256;
                    aes.Key = bKey;

                    using (CryptoStream cryptoStream = new CryptoStream(memory, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        try
                        {
                            byte[] tmp = new byte[encryptedBytes.Length];
                            int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length);
                            byte[] ret = new byte[len];
                            Array.Copy(tmp, 0, ret, 0, len);

                            return Encoding.UTF8.GetString(ret, 0, len);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                }
            }
        }
    }
}
