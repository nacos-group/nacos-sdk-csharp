namespace Nacos.Utilities
{
    using Nacos.Exceptions;

    public static class ParamUtil
    {
        private static char[] validChars = new char[] { '_', '-', '.', ':' };

        public static bool IsValid(string param)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                return false;
            }

            for (int i = 0; i < param.Length; i++)
            {
                char ch = param[i];
                if (char.IsLetterOrDigit(ch))
                {
                    continue;
                }
                else if (IsValidChar(ch))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidChar(char ch)
        {
            foreach (var c in validChars)
            {
                if (c == ch) return true;
            }

            return false;
        }

        public static void CheckKeyParam(string dataId, string group)
        {
            if (string.IsNullOrWhiteSpace(dataId) || !IsValid(dataId))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "dataId invalid");
            }

            if (string.IsNullOrWhiteSpace(group) || !IsValid(group))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "group invalid");
            }
        }

        public static void CheckTDG(string tenant, string dataId, string group)
        {
            if (string.IsNullOrWhiteSpace(dataId) || !IsValid(dataId))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "dataId invalid");
            }

            if (string.IsNullOrWhiteSpace(group) || !IsValid(group))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "group invalid");
            }
        }

        public static void CheckTenant(string tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant) || !IsValid(tenant))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "tenant invalid");
            }
        }

        public static void CheckParam(string dataId, string group, string content)
        {
            CheckKeyParam(dataId, group);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "content invalid");
            }
        }

        public static void CheckIpAndPort(string ip, int port)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "ip invalid");
            }

            if (port <= 0 || port > 65535)
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "port invalid");
            }
        }

        public static void CheckServiceName(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new NacosException(NacosException.CLIENT_INVALID_PARAM, "serviceName invalid");
            }
        }

        public static void CheckInstanceInfo(string ip, int port, string serviceName)
        {
            CheckIpAndPort(ip, port);

            CheckServiceName(serviceName);
        }
    }
}
