namespace Nacos.V2.Utils
{
    using Nacos.V2.Common;
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
            string tmp = EnvUtil.GetEnvValue("tenant.id");

            if (tmp.IsNullOrWhiteSpace())
            {
                tmp = EnvUtil.GetEnvValue("acm.namespace");
            }

            return tmp ?? string.Empty;
        }

        /// <summary>
        /// Adapt the way ANS gets tenant on the cloud.
        /// </summary>
        /// <returns>user tenant for ans</returns>
        public static string GetUserTenantForAns()
        {
            string tmp = EnvUtil.GetEnvValue("tenant.id");

            if (tmp.IsNullOrWhiteSpace())
            {
                tmp = EnvUtil.GetEnvValue("ans.namespace");
            }

            return tmp;
        }
    }
}
