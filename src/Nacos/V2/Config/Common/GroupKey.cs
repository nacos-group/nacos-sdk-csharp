namespace Nacos.V2.Config
{
    using Nacos.V2.Utils;
    using System.Text;

    public class GroupKey
    {
        private static readonly char PLUS = '+';

        private static readonly char PERCENT = '%';

        private static readonly char TWO = '2';

        private static readonly char B = 'B';

        private static readonly char FIVE = '5';

        public static string GetKey(string dataId, string group) => GetKey(dataId, group, "");

        public static string GetKey(string dataId, string group, string datumStr) => DoGetKey(dataId, group, datumStr);

        public static string GetKeyTenant(string dataId, string group, string tenant) => DoGetKey(dataId, group, tenant);

        private static string DoGetKey(string dataId, string group, string datumStr)
        {
            StringBuilder sb = new StringBuilder();
            UrlEncode(dataId, sb);
            sb.Append(PLUS);
            UrlEncode(group, sb);
            if (datumStr.IsNotNullOrWhiteSpace())
            {
                sb.Append(PLUS);
                UrlEncode(datumStr, sb);
            }

            return sb.ToString();
        }

        public static string[] ParseKey(string groupKey)
        {
            StringBuilder sb = new StringBuilder();
            string dataId = null;
            string group = null;
            string tenant = null;

            var arr = groupKey.ToCharArray();

            for (int i = 0; i < groupKey.Length; ++i)
            {
                char c = arr[i];
                if (c == PLUS)
                {
                    if (dataId == null)
                    {
                        dataId = sb.ToString();
                        sb.Length = 0;
                    }
                    else if (group == null)
                    {
                        group = sb.ToString();
                        sb.Length = 0;
                    }
                    else
                    {
                        throw new System.ArgumentException("invalid groupkey:" + groupKey);
                    }
                }
                else if (c == PERCENT)
                {
                    char next = arr[++i];
                    char nextnext = arr[++i];
                    if (next == TWO && nextnext == B)
                    {
                        sb.Append(PLUS);
                    }
                    else if (next == TWO && nextnext == FIVE)
                    {
                        sb.Append(PERCENT);
                    }
                    else
                    {
                        throw new System.ArgumentException("invalid groupkey:" + groupKey);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (group.IsNullOrWhiteSpace())
            {
                group = sb.ToString();
                if (group.Length == 0)
                {
                    throw new System.ArgumentException("invalid groupkey:" + groupKey);
                }
            }
            else
            {
                tenant = sb.ToString();
                if (group.Length == 0)
                {
                    throw new System.ArgumentException("invalid groupkey:" + groupKey);
                }
            }

            return new string[] { dataId, group, tenant };
        }

        /// <summary>
        /// + -> %2B % -> %25.
        /// </summary>
        /// <param name="str">str</param>
        /// <param name="sb">StringBuilder</param>
        private static void UrlEncode(string str, StringBuilder sb)
        {
            var arr = str.ToCharArray();

            for (int idx = 0; idx < str.Length; ++idx)
            {
                char c = arr[idx];
                if (c == PLUS)
                {
                    sb.Append("%2B");
                }
                else if (c == PERCENT)
                {
                    sb.Append("%25");
                }
                else
                {
                    sb.Append(c);
                }
            }
        }
    }
}
