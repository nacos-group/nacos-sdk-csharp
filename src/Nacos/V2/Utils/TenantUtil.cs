namespace Nacos.V2.Utils
{
    using System;

    public class TenantUtil
    {
        /// <summary>
        /// Adapt the way ACM gets tenant on the cloud.
        /// Note the difference between getting and getting ANS. Since the processing logic on the server side is different,
        /// the default value returns differently.
        /// </summary>
        /// <returns>user tenant for acm</returns>
        public static string GetUserTenantForAcm()
        {
            string tmp = Environment.GetEnvironmentVariable("tenant.id");

            if (string.IsNullOrWhiteSpace(tmp))
            {
                tmp = Environment.GetEnvironmentVariable("acm.namespace");
            }

            return tmp ?? string.Empty;
        }

        /// <summary>
        /// Adapt the way ANS gets tenant on the cloud.
        /// </summary>
        /// <returns>user tenant for ans</returns>
        public static string GetUserTenantForAns()
        {
            string tmp = Environment.GetEnvironmentVariable("tenant.id");

            if (string.IsNullOrWhiteSpace(tmp))
            {
                tmp = Environment.GetEnvironmentVariable("ans.namespace");
            }

            return tmp;
        }
    }
}
