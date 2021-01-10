namespace Nacos.V2.Naming.Cache
{
    using Nacos.V2.Common;
    using Nacos.V2.Naming.Dtos;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class DiskCache
    {
        public static async Task WriteAsync(ServiceInfo dom, string dir)
        {
            try
            {
                MakeSureCacheDirExists(dir);

                var fileName = dom.GetKeyEncoded();

                var json = dom.JsonFromServer;

                if (json.IsNullOrWhiteSpace()) json = dom.ToJsonString();

                var path = Path.Combine(dir, fileName);

                using FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await fs.WriteAsync(bytes, 0, bytes.Length);
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task<Dictionary<string, ServiceInfo>> ReadAsync(string cacheDir)
        {
            Dictionary<string, ServiceInfo> domMap = new Dictionary<string, ServiceInfo>(16);

            try
            {
                var files = MakeSureCacheDirExists(cacheDir);

                foreach (string filePath in files)
                {
                    var fileName = System.Net.WebUtility.UrlDecode(filePath);

                    if (!(fileName.EndsWith(Constants.SERVICE_INFO_SPLITER + "meta") || fileName.EndsWith(Constants.SERVICE_INFO_SPLITER + "special-url")))
                    {
                        ServiceInfo dom = new ServiceInfo(fileName);
                        List<Instance> ips = new List<Instance>();
                        dom.Hosts = ips;

                        ServiceInfo newFormat = null;

                        using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        byte[] readByte = new byte[fs.Length];
                        await fs.ReadAsync(readByte, 0, readByte.Length);
                        string readStr = Encoding.UTF8.GetString(readByte);
                        fs.Close();

                        using StringReader sr = new StringReader(readStr);
                        while (true)
                        {
                            var line = await sr.ReadLineAsync();
                            if (line == null || line.Length <= 0)
                                break;

                            try
                            {
                                if (!line.StartsWith("{")) continue;

                                newFormat = line.ToObj<ServiceInfo>();

                                if (string.IsNullOrWhiteSpace(newFormat.Name))
                                {
                                    ips.Add(line.ToObj<Instance>());
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }

                        if (newFormat != null
                            && !string.IsNullOrWhiteSpace(newFormat.Name)
                            && newFormat.Hosts != null && newFormat.Hosts.Any())
                        {
                            domMap[dom.GetKey()] = newFormat;
                        }
                        else if (dom.Hosts != null && dom.Hosts.Any())
                        {
                            domMap[dom.GetKey()] = dom;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return domMap;
        }

        private static string[] MakeSureCacheDirExists(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Directory.GetFiles(dir);
        }
    }
}
