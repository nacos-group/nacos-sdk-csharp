namespace Nacos.Auth.Ram.Utils
{
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;

    internal class SpasAdapter
    {
        private static readonly string TIMESTAMP_HEADER = "Timestamp";

        private static readonly string SIGNATURE_HEADER = "Spas-Signature";

        private static readonly string GROUP_KEY = "group";

        public static readonly string TENANT_KEY = "tenant";

        public static Dictionary<string, string> GetSignHeaders(string resource, string secretKey)
        {
            Dictionary<string, string> header = new(2);

            string timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

            header[TIMESTAMP_HEADER] = timeStamp;

            if (secretKey != null)
            {
                string signature = resource.IsNullOrWhiteSpace()
                    ? SignUtil.Sign(timeStamp, secretKey)
                    : SignUtil.Sign(string.Concat(resource, "+", timeStamp), secretKey);

                header[SIGNATURE_HEADER] = signature;
            }

            return header;
        }

        public static Dictionary<string, string> GetSignHeaders(string groupKey, string tenant, string secretKey)
        {
            if (groupKey.IsNullOrWhiteSpace()
                && tenant.IsNullOrWhiteSpace())
            {
                return null;
            }

            string resource = "";
            if (groupKey.IsNotNullOrWhiteSpace()
                && tenant.IsNotNullOrWhiteSpace())
            {
                resource = string.Concat(tenant, "+", groupKey);
            }
            else
            {
                if (groupKey.IsNotNullOrWhiteSpace())
                {
                    resource = groupKey;
                }
            }

            return GetSignHeaders(resource, secretKey);
        }

        public static Dictionary<string, string> GetSignHeaders(Dictionary<string, string> paramValues, string secretKey)
        {
            if (paramValues == null)
            {
                return null;
            }

            string resource = "";
            if (paramValues.TryGetValue(TENANT_KEY, out var tenant)
                && paramValues.TryGetValue(GROUP_KEY, out var group))
            {
                resource = string.Concat(tenant, "+", group);
            }
            else
            {
                if (paramValues.TryGetValue(GROUP_KEY, out group) && group.IsNotNullOrWhiteSpace())
                {
                    resource = group;
                }
            }

            return GetSignHeaders(resource, secretKey);
        }
    }
}
