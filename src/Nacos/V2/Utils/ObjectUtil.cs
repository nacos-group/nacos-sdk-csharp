namespace Nacos.V2.Utils
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public static class ObjectUtil
    {
        public static long ToTimestamp(this DateTime input)
            => (long)(input - TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Utc)).TotalSeconds;

        public static string ToJsonString(this object obj)
            => JsonConvert.SerializeObject(obj);

        public static T ToObj<T>(this string json)
            => json.IsNullOrWhiteSpace() ? default(T) : JsonConvert.DeserializeObject<T>(json);

        public static object ToObj(this string json, Type type)
            => json.IsNullOrWhiteSpace() || type == null ? null : JsonConvert.DeserializeObject(json, type);

        public static string SafeGetValue(this System.Collections.Generic.Dictionary<string, object> dict, string key, string defaultVal = "")
            => dict.TryGetValue(key, out var val) ? (string)val : defaultVal;

        public static async Task<string> ReadFileAsync(this FileInfo file)
        {
            using FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] readByte = new byte[fs.Length];
            await fs.ReadAsync(readByte, 0, readByte.Length).ConfigureAwait(false);
            string readStr = Encoding.UTF8.GetString(readByte);
            fs.Close();
            return readStr;
        }
    }
}
