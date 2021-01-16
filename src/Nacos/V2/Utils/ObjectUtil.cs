namespace Nacos.V2.Utils
{
    using System;
    using System.Threading;

    public static class ObjectUtil
    {
        public static long DateTimeToTimestamp(DateTime input)
            => (long)(input - TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Utc)).TotalSeconds;
    }
}
