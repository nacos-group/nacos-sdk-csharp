namespace Nacos.Tests.Naming.Utils
{
    using Nacos.Exceptions;
    using Nacos.Naming.Core;
    using Nacos.Naming.Dtos;
    using Nacos.Naming.Utils;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class NamingUtilsTest
    {
        [Fact]
        public void GetGroupedName_Should_Exception()
        {
            Assert.Throws<ArgumentException>(() => NamingUtils.GetGroupedName("", ""));
        }

        [Fact]
        public void GetGroupedName_Should_Successed()
        {
            var gn = NamingUtils.GetGroupedName("svc", "");
            Assert.Equal("@@svc", gn);

            gn = NamingUtils.GetGroupedName("svc", "group");
            Assert.Equal("group@@svc", gn);
        }

        [Fact]
        public void GetServiceName_Should_Successed()
        {
            var serName = NamingUtils.GetServiceName("");
            Assert.Equal(string.Empty, serName);

            serName = NamingUtils.GetServiceName("@svc@");
            Assert.Equal("@svc@", serName);

            serName = NamingUtils.GetServiceName("group@@svc");
            Assert.Equal("svc", serName);

            serName = NamingUtils.GetServiceName("group@@svc@@clusters");
            Assert.Equal("svc", serName);
        }

        [Fact]
        public void GetGroupName_Should_Successed()
        {
            var serName = NamingUtils.GetGroupName("");
            Assert.Equal(string.Empty, serName);

            serName = NamingUtils.GetGroupName("@group@");
            Assert.Equal("DEFAULT_GROUP", serName);

            serName = NamingUtils.GetGroupName("group@@svc");
            Assert.Equal("group", serName);

            serName = NamingUtils.GetGroupName("group@@svc@@clusters");
            Assert.Equal("group", serName);
        }

        [Fact]
        public void CheckServiceNameFormat_Should_Exception()
        {
            Assert.Throws<ArgumentException>(() => NamingUtils.CheckServiceNameFormat(""));

            Assert.Throws<ArgumentException>(() => NamingUtils.CheckServiceNameFormat("group%40svc%40clusters"));
        }

        [Fact]
        public void GetGroupedNameOptional_Should_Successed()
        {
            var gn = NamingUtils.GetGroupedNameOptional("svc", "group");
            Assert.Equal("group@@svc", gn);
        }


        [Fact]
        public void CheckInstanceIsEphemeral_Should_Exception()
        {
            var instance = new Instance
            {
                Ephemeral = false,
            };

            var ex = Assert.Throws<NacosException>(() => NamingUtils.CheckInstanceIsEphemeral(instance));
            Assert.Equal(NacosException.INVALID_PARAM, ex.ErrorCode);
        }

        [Fact]
        public void CheckInstanceIsLegal_Should_Exception()
        {
            var instance = new Instance();
            NamingUtils.CheckInstanceIsLegal(instance);

            instance.Metadata.Add(PreservedMetadataKeys.HEART_BEAT_TIMEOUT, "1");
            var ex = Assert.Throws<NacosException>(() => NamingUtils.CheckInstanceIsLegal(instance));
            Assert.Equal(NacosException.INVALID_PARAM, ex.ErrorCode);
            Assert.Equal("Instance 'heart beat interval' must less than 'heart beat timeout' and 'ip delete timeout'.", ex.ErrorMsg);

            instance.Metadata = new Dictionary<string, string>
            {
                { PreservedMetadataKeys.IP_DELETE_TIMEOUT, "1" }
            };
            ex = Assert.Throws<NacosException>(() => NamingUtils.CheckInstanceIsLegal(instance));
            Assert.Equal(NacosException.INVALID_PARAM, ex.ErrorCode);
            Assert.Equal("Instance 'heart beat interval' must less than 'heart beat timeout' and 'ip delete timeout'.", ex.ErrorMsg);

            instance.Metadata = new Dictionary<string, string>();
            NamingUtils.CheckInstanceIsLegal(instance);

            instance.ClusterName = "Cluster@name2";
            ex = Assert.Throws<NacosException>(() => NamingUtils.CheckInstanceIsLegal(instance));
            Assert.Equal(NacosException.INVALID_PARAM, ex.ErrorCode);
            Assert.Equal($"Instance 'clusterName' should be characters with only 0-9a-zA-Z-. (current: {instance.ClusterName})", ex.ErrorMsg);
        }
    }
}
