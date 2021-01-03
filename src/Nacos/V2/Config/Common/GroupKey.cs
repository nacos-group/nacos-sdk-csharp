namespace Nacos.V2.Config
{
    using System.Text;

    public class GroupKey
    {
        public static string GetKey(string dataId, string group) => GetKey(dataId, group, "");

        public static string GetKey(string dataId, string group, string datumStr) => DoGetKey(dataId, group, datumStr);

        public static string GetKeyTenant(string dataId, string group, string tenant) => DoGetKey(dataId, group, tenant);

        private static string DoGetKey(string dataId, string group, string datumStr)
        {
            StringBuilder sb = new StringBuilder();
            UrlEncode(dataId, sb);
            sb.Append('+');
            UrlEncode(group, sb);
            if (!string.IsNullOrWhiteSpace(datumStr))
            {
                sb.Append('+');
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
                if (c == '+')
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
                else if (c == '%')
                {
                    char next = arr[++i];
                    char nextnext = arr[++i];
                    if (next == '2' && nextnext == 'B')
                    {
                        sb.Append('+');
                    }
                    else if (next == '2' && nextnext == '5')
                    {
                        sb.Append('%');
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

            if (string.IsNullOrWhiteSpace(group))
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
                if (c == '+')
                {
                    sb.Append("%2B");
                }
                else if (c == '%')
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
