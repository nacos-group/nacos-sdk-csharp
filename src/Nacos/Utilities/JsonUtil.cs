namespace Nacos.Utilities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;

    public static class JsonUtil
    {
        public static string ToJsonString(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T ToObj<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return default(T);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static object ToObj(this string json, Type type)
        {
            if (string.IsNullOrWhiteSpace(json) || type == null) return null;

            return JsonConvert.DeserializeObject(json, type);
        }

        public static string GetPropValue(this string json, string prop)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;

            var jObj = JObject.Parse(json);

            return jObj.Value<string>(prop);
        }
    }
}
