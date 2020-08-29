namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class DiskCache
    {
        private readonly ILogger _logger;

        public const string LINE_SEPARATOR = "&#10;";

        public DiskCache(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DiskCache>();
        }

        public async Task WriteFileAsync(string path, string content)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                await fs.WriteAsync(bytes, 0, bytes.Length);
                fs.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NA] failed to write cache for file: {0}", path);
            }
        }

        public async Task WriteServiceInfoAsync(string dir, ServiceInfo info)
        {
            try
            {
                MakeSureCacheDirExists(dir);

                var fileName = info.GetKeyEncoded();
                var content = info.ToJsonString();

                await WriteFileAsync(Path.Combine(dir, fileName), content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NA] failed to write cache for dom: {0}", info.name);
            }
        }

        public string GetLineSeparator()
        {
            return LINE_SEPARATOR;
        }

        public bool IsFileExists(string path)
        {
            return File.Exists(path);
        }

        public long GetFileLastModifiedTime(string path)
        {
            var dt = File.GetLastWriteTime(path);
            return Convert.ToInt64((dt - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
        }

        public async Task<Dictionary<string, ServiceInfo>> GetServiceInfosAsync(string dir)
        {
            var infos = new Dictionary<string, ServiceInfo>();

            try
            {
                var files = MakeSureCacheDirExists(dir);

                foreach (string filePath in files)
                {
                    var fileName = System.Net.WebUtility.UrlDecode(filePath);

                    if (!(fileName.EndsWith(ConstValue.ServiceInfoSplitter + "meta") || fileName.EndsWith(ConstValue.ServiceInfoSplitter + "special-url")))
                    {
                        var content = await ReadFile(filePath);
                        var info = content.ToObj<ServiceInfo>();
                        infos.Add(info.GetKey(), info);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NA] failed to read cache file");
            }

            return infos;
        }

        public async Task<string> ReadFile(string path)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] readByte = new byte[fs.Length];
                await fs.ReadAsync(readByte, 0, readByte.Length);
                string readStr = Encoding.UTF8.GetString(readByte);
                fs.Close();
                return readStr;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[NA] failed to read cache for file: {0}", path);
            }

            return string.Empty;
        }

        public string[] MakeSureCacheDirExists(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Directory.GetFiles(dir);
        }
    }
}