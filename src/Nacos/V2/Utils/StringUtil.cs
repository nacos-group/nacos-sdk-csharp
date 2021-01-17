namespace Nacos.V2.Utils
{
    using System;

    public static class StringUtil
    {
        public static string[] SplitByString(this string str, string separator)
            => str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);

        public static bool IsNullOrWhiteSpace(this string str)
            => string.IsNullOrWhiteSpace(str);

        public static bool IsNotNullOrWhiteSpace(this string str)
            => !string.IsNullOrWhiteSpace(str);
    }
}
