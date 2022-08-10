namespace Nacos.V2.Naming.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Nacos.V2.Common;
    using Nacos.V2.Exceptions;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Utils;

    public class NamingUtils
    {
        /// <summary>
        /// Returns a combined string with serviceName and groupName. serviceName can not be nil.
        /// In most cases, serviceName can not be nil. In other cases, for search or anything, See {@link * com.alibaba.nacos.api.naming.utils.NamingUtils#getGroupedNameOptional(String, String)}
        /// etc:
        /// serviceName | groupName | result
        /// serviceA    | groupA    | groupA@@serviceA
        /// nil         | groupA    | threw ArgumentException
        /// </summary>
        /// <param name="serviceName">name of service</param>
        /// <param name="groupName">name of group</param>
        /// <returns>groupName@@serviceName</returns>
        public static string GetGroupedName(string serviceName, string groupName)
        {
            if (serviceName.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Param 'serviceName' is illegal, serviceName is blank");
            }

            string resultGroupedName = groupName + Constants.SERVICE_INFO_SPLITER + serviceName;
            return resultGroupedName;
        }

        public static string GetServiceName(string serviceNameWithGroup)
        {
            if (serviceNameWithGroup.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            if (!serviceNameWithGroup.Contains(Constants.SERVICE_INFO_SPLITER))
            {
                return serviceNameWithGroup;
            }

            return serviceNameWithGroup.SplitByString(Constants.SERVICE_INFO_SPLITER)[1];
        }

        public static string GetGroupName(string serviceNameWithGroup)
        {
            if (serviceNameWithGroup.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            if (!serviceNameWithGroup.Contains(Constants.SERVICE_INFO_SPLITER))
            {
                return Constants.DEFAULT_GROUP;
            }

            return serviceNameWithGroup.SplitByString(Constants.SERVICE_INFO_SPLITER)[0];
        }

        /// <summary>
        /// check combineServiceName format. the serviceName can't be blank.
        /// <pre>
        /// serviceName = "@@"; the length = 0; illegal
        /// serviceName = "group@@"; the length = 1; illegal
        /// serviceName = "@@serviceName"; the length = 2; legal
        /// serviceName = "group@@serviceName"; the length = 2; legal
        /// </pre>
        /// </summary>
        /// <param name="combineServiceName">such as: groupName@@serviceName</param>
        public static void CheckServiceNameFormat(string combineServiceName)
        {
            string[] split = combineServiceName.SplitByString(Constants.SERVICE_INFO_SPLITER);
            if (split.Length <= 1)
            {
                throw new ArgumentException(
                        "Param 'serviceName' is illegal, it should be format as 'groupName@@serviceName'");
            }
        }

        /// <summary>
        /// Returns a combined string with serviceName and groupName. Such as 'groupName@@serviceName'
        /// This method works similar with {@link com.alibaba.nacos.api.naming.utils.NamingUtils#getGroupedName} But not verify any parameters.
        /// etc:
        /// serviceName | groupName | result
        /// serviceA    | groupA    | groupA@@serviceA
        /// nil         | groupA    | groupA@@
        /// nil         | nil       | @@
        /// </summary>
        /// <param name="serviceName">serviceName</param>
        /// <param name="groupName">groupName</param>
        /// <returns>groupName@@serviceName</returns>
        public static string GetGroupedNameOptional(string serviceName, string groupName)
        {
            return groupName + Constants.SERVICE_INFO_SPLITER + serviceName;
        }

        /// <summary>
        /// Batch verify the validity of instances.
        /// </summary>
        /// <param name="instances">List of instances to be registered</param>
        public static void BatchCheckInstanceIsLegal(List<Instance> instances)
        {
            HashSet<Instance> newInstanceSet = new HashSet<Instance>(instances);

            foreach (var instance in newInstanceSet)
            {
                CheckInstanceIsEphemeral(instance);
                CheckInstanceIsLegal(instance);
            }
        }

        /// <summary>
        /// check batch register is Ephemeral.
        /// </summary>
        /// <param name="instance">instance</param>
        /// <exception cref="NacosException"></exception>
        public static void CheckInstanceIsEphemeral(Instance instance)
        {
            if (!instance.Ephemeral)
            {
                throw new NacosException(
                    NacosException.INVALID_PARAM,
                    $"Batch registration does not allow persistent instance registration , Instance：{instance}");
            }
        }

        /// <summary>
        /// Check instance param about keep alive.
        /// heart beat timeout must > heart beat interval
        /// ip delete timeout must  > heart beat interval
        /// </summary>
        /// <param name="instance">need checked instance</param>
        /// <exception cref="NacosException"></exception>
        public static void CheckInstanceIsLegal(Instance instance)
        {
            if (instance.GetInstanceHeartBeatTimeOut() < instance.GetInstanceHeartBeatInterval()
                    || instance.GetIpDeleteTimeout() < instance.GetInstanceHeartBeatInterval())
            {
                throw new NacosException(
                    NacosException.INVALID_PARAM,
                    "Instance 'heart beat interval' must less than 'heart beat timeout' and 'ip delete timeout'.");
            }

            if (instance.ClusterName.IsNotNullOrWhiteSpace()
                && !Regex.IsMatch(instance.ClusterName, Common.Constants.CLUSTER_NAME_PATTERN_STRING))
            {
                throw new NacosException(
                    NacosException.INVALID_PARAM,
                    $"Instance 'clusterName' should be characters with only 0-9a-zA-Z-. (current: {instance.ClusterName})");
            }
        }
    }
}
