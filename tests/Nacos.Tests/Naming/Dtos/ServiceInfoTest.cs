namespace Nacos.Tests.Naming.Dtos
{
    using Nacos.Naming.Dtos;
    using System.Collections.Generic;
    using Xunit;

    public class ServiceInfoTest
    {
        private ServiceInfo serviceInfo;

        public ServiceInfoTest()
        {
            serviceInfo = new ServiceInfo("My_GROUP@@mysvc2");
        }

        [Fact]
        public void IpCount_Should_Successed()
        {
            var count = serviceInfo.IpCount();
            Assert.Equal(0, count);

            serviceInfo.Hosts = new List<Instance> { new Instance(), new Instance() };
            count = serviceInfo.IpCount();
            Assert.Equal(2, count);
        }

        [Fact]
        public void GetKey_Should_Successed()
        {
            var key = serviceInfo.GetKey();
            Assert.Equal($"My_GROUP@@mysvc2", key);

            var serInfo = new ServiceInfo();
            serInfo.Name = "svc";
            key = serInfo.GetKey();
            Assert.Equal("svc", key);

            serInfo.GroupName = "group";
            key = serInfo.GetKey();
            Assert.Equal("group@@svc", key);

            serInfo.Clusters = "clusters";
            key = serInfo.GetKey();
            Assert.Equal("group@@svc@@clusters", key);
        }

        [Fact]
        public void GetKeyEncoded_Should_Successed()
        {
            var key = serviceInfo.GetKeyEncoded();
            Assert.Equal($"My_GROUP%40%40mysvc2", key);

            var serInfo = new ServiceInfo();
            serInfo.Name = "svc";
            key = serInfo.GetKeyEncoded();
            Assert.Equal("svc", key);

            serInfo.GroupName = "group";
            key = serInfo.GetKeyEncoded();
            Assert.Equal("group%40%40svc", key);

            serInfo.Clusters = "clusters";
            key = serInfo.GetKeyEncoded();
            Assert.Equal("group%40%40svc%40%40clusters", key);
        }

        [Fact]
        public void Validate_Should_Successed()
        {
            var serviceInfo = new ServiceInfo();
            Assert.False(serviceInfo.Validate());

            serviceInfo.Hosts = new List<Instance> { new Instance { Healthy = false } };
            Assert.False(serviceInfo.Validate());

            serviceInfo.Hosts.Add(new Instance { Healthy = true, Weight = 0 });
            Assert.False(serviceInfo.Validate());

            serviceInfo.AllIPs = true;
            Assert.True(serviceInfo.Validate());

            serviceInfo.AllIPs = false;
            Assert.False(serviceInfo.Validate());

            serviceInfo.Hosts.Add(new Instance { Healthy = true, Weight = 1 });
            Assert.True(serviceInfo.Validate());
        }

        [Fact]
        public void FromKey_Should_Successed()
        {
            var serviceInfo = ServiceInfo.FromKey("");
            Assert.NotNull(serviceInfo);
            Assert.Null(serviceInfo.GroupName);
            Assert.Null(serviceInfo.Name);
            Assert.Null(serviceInfo.Clusters);

            var serviceInfo1 = ServiceInfo.FromKey("group--svc");
            Assert.NotNull(serviceInfo1);
            Assert.Null(serviceInfo1.GroupName);
            Assert.Null(serviceInfo1.Name);
            Assert.Null(serviceInfo1.Clusters);

            var serviceInfo3 = ServiceInfo.FromKey("group@@svc");
            Assert.NotNull(serviceInfo3);
            Assert.Equal("group", serviceInfo3.GroupName);
            Assert.Equal("svc", serviceInfo3.Name);
            Assert.Null(serviceInfo3.Clusters);

            var serviceInfo4 = ServiceInfo.FromKey("group@@svc@@clusters");
            Assert.NotNull(serviceInfo4);
            Assert.Equal("group", serviceInfo4.GroupName);
            Assert.Equal("svc", serviceInfo4.Name);
            Assert.Equal("clusters", serviceInfo4.Clusters);
        }
    }
}
