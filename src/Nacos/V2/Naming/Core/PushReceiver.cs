namespace Nacos.V2.Naming.Core
{
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Remote;
    using System.Collections.Generic;

    public class PushReceiver : System.IDisposable
    {
        private ServiceInfoHolder _serviceInfoHolder;

        public PushReceiver(ServiceInfoHolder serviceInfoHolder)
        {
            this._serviceInfoHolder = serviceInfoHolder;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
