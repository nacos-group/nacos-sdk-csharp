namespace Nacos.V2.Naming.Backups
{
    using Nacos.V2.Naming.Cache;
    using System.Collections.Concurrent;

    public class FailoverReactor
    {
        private string failoverDir;

        private ServiceInfoHolder serviceInfoHolder;

        public FailoverReactor(ServiceInfoHolder serviceInfoHolder, string cacheDir)
        {
            this.serviceInfoHolder = serviceInfoHolder;
            this.failoverDir = cacheDir + "/failover";

            this.Init();
        }

        private void Init()
        {
        }

        private ConcurrentDictionary<string, ServiceInfo> serviceMap = new ConcurrentDictionary<string, ServiceInfo>();

        private ConcurrentDictionary<string, string> switchParams = new ConcurrentDictionary<string, string>();

        /*private static readonly long DAY_PERIOD_MINUTES = 24 * 60;*/
    }
}
