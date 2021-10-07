namespace Nacos.Tests.Naming.Cache
{
    using Microsoft.Extensions.Logging.Abstractions;
    using Nacos.V2;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Naming.Dtos;
    using Nacos.V2.Naming.Event;
    using System.Threading.Tasks;
    using Xunit;

    public class ServiceInfoHolderTests
    {
        [Fact]
        public void Udp_ProcessServiceInfo_Should_Succeed()
        {
            InstancesChangeNotifier notifier = new InstancesChangeNotifier();
            NacosSdkOptions options = new NacosSdkOptions() { NamingUseRpc = false };
            ServiceInfoHolder holder = new ServiceInfoHolder(NullLogger.Instance, "", options, notifier);

            var listener = new CusListener();
            var svcName = "mysvc2";
            var groupName = "My_GROUP";
            var clusterName = "DEFAULT";

            notifier.RegisterListener(groupName, svcName, clusterName, listener);

            var json = "{\"name\":\"My_GROUP@@mysvc2\",\"clusters\":\"\",\"cacheMillis\":10000,\"hosts\":[{\"instanceId\":\"127.0.0.1#9563#DEFAULT#My_GROUP@@mysvc2\",\"ip\":\"127.0.0.1\",\"port\":9563,\"weight\":1.0,\"healthy\":true,\"enabled\":true,\"ephemeral\":true,\"clusterName\":\"DEFAULT\",\"serviceName\":\"My_GROUP@@mysvc2\",\"metadata\":{},\"instanceHeartBeatTimeOut\":15000,\"ipDeleteTimeout\":30000,\"instanceIdGenerator\":\"simple\",\"instanceHeartBeatInterval\":5000}],\"lastRefTime\":1633588038730,\"checksum\":\"\",\"allIPs\":false,\"reachProtectionThreshold\":false,\"valid\":true}";
            holder.ProcessServiceInfo(json);
        }

        [Fact]
        public void Grpc_ProcessServiceInfo_Should_Succeed()
        {
            InstancesChangeNotifier notifier = new InstancesChangeNotifier();
            NacosSdkOptions options = new NacosSdkOptions() { NamingUseRpc = true };
            ServiceInfoHolder holder = new ServiceInfoHolder(NullLogger.Instance, "", options, notifier);

            var listener = new CusListener();
            var svcName = "mysvc2";
            var groupName = "My_GROUP";
            var clusterName = "DEFAULT";

            notifier.RegisterListener(groupName, svcName, clusterName, listener);

            ServiceInfo info = new ServiceInfo();
            info.AllIPs = false;
            info.CacheMillis = 10000;
            info.Checksum = string.Empty;
            info.Clusters = "";
            info.GroupName = groupName;
            info.LastRefTime = 0;
            info.Name = svcName;
            info.Hosts = new System.Collections.Generic.List<Instance>()
            {
                new Instance { ServiceName = "My_GROUP@@mysvc2" }
            };

            holder.ProcessServiceInfo(info);
        }

        public class CusListener : Nacos.V2.IEventListener
        {
            public Task OnEvent(Nacos.V2.IEvent @event)
            {
                if (@event is Nacos.V2.Naming.Event.InstancesChangeEvent e)
                {
                    Assert.Equal("mysvc2", e.ServiceName);
                    Assert.Equal("My_GROUP", e.GroupName);
                }

                return Task.CompletedTask;
            }
        }
    }
}
