namespace Nacos.Naming.Cache
{
    using System;
    using System.Collections.Concurrent;

    public class ServiceInfoHolder
    {
        private ConcurrentDictionary<string, ServiceInfo> serviceInfoMap;

        private Nacos.Naming.Backups.FailoverReactor failoverReactor;

        private string cacheDir = "";

        public ServiceInfoHolder(string @namespace, NacosOptions nacosOptions)
        {
            InitCacheDir(@namespace);

            if (IsLoadCacheAtStart(nacosOptions))
            {
                this.serviceInfoMap = new ConcurrentDictionary<string, ServiceInfo>();
            }
            else
            {
                this.serviceInfoMap = new ConcurrentDictionary<String, ServiceInfo>();
            }

            this.failoverReactor = new Nacos.Naming.Backups.FailoverReactor(this, cacheDir);
        }

        private bool IsLoadCacheAtStart(NacosOptions nacosOptions)
        {
            throw new NotImplementedException();
        }

        private void InitCacheDir(string @namespace)
        {
            throw new NotImplementedException();
        }
    }
}
