namespace Nacos.Tests.Naming.Dtos
{
    using Nacos.Common;
    using Nacos.Naming.Core;
    using Nacos.Naming.Dtos;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class InstanceTest
    {
        private Instance instance;

        public InstanceTest()
        {
            instance = new Instance
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                Ip = "192.168.0.1",
                Port = 10101,
                Weight = 1,
                ClusterName = "DEFAULT",
                ServiceName = "DEFAULT-1",
                Metadata = new Dictionary<string, string>
                {
                    { "aa", "bb" },
                    { "cc", "dd" }
                }
            };
        }

        [Fact]
        public void AddMetadata_Should_Successed()
        {
            instance.AddMetadata("ee", "ff");
            Assert.Equal(3, instance.Metadata.Count);
            Assert.Equal("ee", instance.Metadata.LastOrDefault().Key);
            Assert.Equal("ff", instance.Metadata.LastOrDefault().Value);

            instance.Metadata.Remove("ee", out var value);
            Assert.Equal(2, instance.Metadata.Count);
        }

        [Fact]
        public void ToInetAddr_Should_Successed()
        {
            var addr = instance.ToInetAddr();
            Assert.Equal("192.168.0.1:10101", addr);
        }

        [Fact]
        public void GetInstanceHeartBeatInterval_Should_Successed()
        {
            // get default value
            var value = instance.GetInstanceHeartBeatInterval();
            Assert.Equal(Constants.DEFAULT_HEART_BEAT_INTERVAL, value);

            // get value of set long
            var metaValue = 10;
            instance.AddMetadata(PreservedMetadataKeys.HEART_BEAT_INTERVAL, metaValue.ToString());
            value = instance.GetInstanceHeartBeatInterval();
            Assert.Equal(metaValue, value);

            // get value of set char
            instance.Metadata[PreservedMetadataKeys.HEART_BEAT_INTERVAL] = "12345qq";
            value = instance.GetInstanceHeartBeatInterval();
            Assert.Equal(Constants.DEFAULT_HEART_BEAT_INTERVAL, value);
        }

        [Fact]
        public void GetInstanceHeartBeatTimeOut_Should_Successed()
        {
            // get default value
            var value = instance.GetInstanceHeartBeatTimeOut();
            Assert.Equal(Constants.DEFAULT_HEART_BEAT_TIMEOUT, value);

            // get set value
            var metaValue = 11;
            instance.AddMetadata(PreservedMetadataKeys.HEART_BEAT_TIMEOUT, metaValue.ToString());

            value = instance.GetInstanceHeartBeatTimeOut();
            Assert.Equal(metaValue, value);
        }

        [Fact]
        public void GetIpDeleteTimeout_Should_Successed()
        {
            // get default value
            var value = instance.GetIpDeleteTimeout();
            Assert.Equal(Constants.DEFAULT_IP_DELETE_TIMEOUT, value);

            // get set value
            var metaValue = 12;
            instance.AddMetadata(PreservedMetadataKeys.IP_DELETE_TIMEOUT, metaValue.ToString());

            value = instance.GetIpDeleteTimeout();
            Assert.Equal(metaValue, value);
        }

        [Fact]
        public void GetInstanceIdGenerator_Should_Successed()
        {
            // get default value
            var value = instance.GetInstanceIdGenerator();
            Assert.Equal(Constants.DEFAULT_INSTANCE_ID_GENERATOR, value);

            // get set value
            var metaValue = "13";
            instance.AddMetadata(PreservedMetadataKeys.INSTANCE_ID_GENERATOR, metaValue);

            value = instance.GetInstanceIdGenerator();
            Assert.Equal(metaValue, value);
        }
    }
}
