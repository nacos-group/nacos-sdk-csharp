namespace Nacos.V2.Config.Utils
{
    using Nacos.V2.Common;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ParamUtils
    {
        private static readonly char[] VALID_CHARS = new char[] { '_', '-', '.', ':' };

        private static readonly string CONTENT_INVALID_MSG = "content invalid";

        private static readonly string DATAID_INVALID_MSG = "dataId invalid";

        private static readonly string TENANT_INVALID_MSG = "tenant invalid";

        private static readonly string BETAIPS_INVALID_MSG = "betaIps invalid";

        private static readonly string GROUP_INVALID_MSG = "group invalid";

        private static readonly string DATUMID_INVALID_MSG = "datumId invalid";

        /// <summary>
        /// Check the whitelist method, the legal parameters can only contain letters, numbers, and characters in validChars, and cannot be empty.
        /// </summary>
        /// <param name="param">parameter</param>
        /// <returns>true if valid</returns>
        public static bool IsValid(String param)
        {
            if (param == null) return false;

            int length = param.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = param[i];
                if (!char.IsLetterOrDigit(ch) && !IsValidChar(ch)) return false;
            }

            return true;
        }

        private static bool IsValidChar(char ch)
        {
            foreach (char c in VALID_CHARS)
            {
                if (c == ch) return true;
            }

            return false;
        }

        /// <summary>
        /// Check Tenant, dataId and group.
        /// </summary>
        /// <param name="tenant">tenant</param>
        /// <param name="dataId">dataId</param>
        /// <param name="group">group</param>
        public static void CheckTdg(string tenant, string dataId, string group)
        {
            CheckTenant(tenant);

            if (dataId.IsNullOrWhiteSpace() || !IsValid(dataId))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, DATAID_INVALID_MSG);
            }

            if (group.IsNullOrWhiteSpace() || !IsValid(group))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, GROUP_INVALID_MSG);
            }
        }

        /// <summary>
        /// Check key param.
        /// </summary>
        /// <param name="dataId">dataId</param>
        /// <param name="group">group</param>
        public static void CheckKeyParam(string dataId, string group)
        {
            if (dataId.IsNullOrWhiteSpace() || !IsValid(dataId))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, DATAID_INVALID_MSG);
            }

            if (group.IsNullOrWhiteSpace() || !IsValid(group))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, GROUP_INVALID_MSG);
            }
        }

        /// <summary>
        /// Check key param.
        /// </summary>
        /// <param name="dataId">dataId</param>
        /// <param name="group">group</param>
        /// <param name="datumId">datumId</param>
        public static void CheckKeyParam(string dataId, string group, string datumId)
        {
            if (dataId.IsNullOrWhiteSpace() || !IsValid(dataId))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, DATAID_INVALID_MSG);
            }

            if (group.IsNullOrWhiteSpace() || !IsValid(group))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, GROUP_INVALID_MSG);
            }

            if (datumId.IsNullOrWhiteSpace() || !IsValid(datumId))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, DATUMID_INVALID_MSG);
            }
        }

        /// <summary>
        /// Check key param.
        /// </summary>
        /// <param name="dataIds">dataIds</param>
        /// <param name="group">group</param>
        public static void CheckKeyParam(List<string> dataIds, string group)
        {
            if (dataIds == null || !dataIds.Any())
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, DATAID_INVALID_MSG);
            }

            foreach (var dataId in dataIds)
            {
                if (dataId.IsNullOrWhiteSpace() || !IsValid(dataId))
                {
                    throw new NacosException(NacosException.CLIENT_INVALID_PARAM, DATAID_INVALID_MSG);
                }
            }

            if (group.IsNullOrWhiteSpace() || !IsValid(group))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, GROUP_INVALID_MSG);
            }
        }

        /// <summary>
        /// Check parameter.
        /// </summary>
        /// <param name="dataId">dataId</param>
        /// <param name="group">group</param>
        /// <param name="content">content</param>
        public static void CheckParam(string dataId, string group, string content)
        {
            CheckKeyParam(dataId, group);
            if (content.IsNullOrWhiteSpace())
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, CONTENT_INVALID_MSG);
            }
        }

        /// <summary>
        /// Check parameter.
        /// </summary>
        /// <param name="dataId">dataId</param>
        /// <param name="group">group</param>
        /// <param name="datumId">datumId</param>
        /// <param name="content">content</param>
        public static void CheckParam(string dataId, string group, string datumId, string content)
        {
            CheckKeyParam(dataId, group, datumId);
            if (content.IsNullOrWhiteSpace())
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, CONTENT_INVALID_MSG);
            }
        }

        /// <summary>
        /// Check Tenant.
        /// </summary>
        /// <param name="tenant">tenant</param>
        public static void CheckTenant(string tenant)
        {
            if (tenant.IsNullOrWhiteSpace() || !IsValid(tenant))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, TENANT_INVALID_MSG);
            }
        }

        /// <summary>
        ///  Check beta ips.
        /// </summary>
        /// <param name="betaIps">beta ips</param>
        public static void CheckBetaIps(string betaIps)
        {
            if (betaIps.IsNullOrWhiteSpace())
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, BETAIPS_INVALID_MSG);
            }

            string[] ipsArr = betaIps.Split(',');
            foreach (var ip in ipsArr)
            {
                if (!IPUtil.IsIP(ip))
                {
                    throw new NacosException(NacosException.CLIENT_INVALID_PARAM, BETAIPS_INVALID_MSG);
                }
            }
        }

        /// <summary>
        /// Check content.
        /// </summary>
        /// <param name="content">content</param>
        public static void CheckContent(string content)
        {
            if (content.IsNullOrWhiteSpace())
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, CONTENT_INVALID_MSG);
            }
        }

        /// <summary>
        /// check whether still using http .
        /// </summary>
        /// <returns>use http or not</returns>
        public static bool UseHttpSwitch()
        {
            var useHttpSwitch = EnvUtil.GetEnvValue("clientworker.use.http.switch");
            return "Y".Equals(useHttpSwitch, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// get connection type for remote.
        /// </summary>
        /// <returns>connection type</returns>
        public static string ConfigRemoteConnectionType()
        {
            string remoteConnectionType = EnvUtil.GetEnvValue("nacos.remote.config.connectiontype");
            return remoteConnectionType;
        }

        public static string Null2DefaultGroup(string group) => (group == null) ? Constants.DEFAULT_GROUP : group.Trim();
    }
}
