namespace Nacos.V2.Utils
{
    using Nacos.V2.Common;
    using Newtonsoft.Json;
    using System;
    using System.Threading;

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
    }
}
