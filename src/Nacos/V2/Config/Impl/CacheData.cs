namespace Nacos.V2.Config.Impl
{
    using Nacos.V2.Common;
    using Nacos.V2.Config.FilterImpl;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;

    public class CacheData
    {
        public static readonly int PerTaskConfigSize = 3000;

        public CacheData()
        {
        }

        public CacheData(ConfigFilterChainManager configFilterChainManager, string name, string dataId, string group)
        {
            if (dataId == null || group == null)
            {
                throw new ArgumentNullException("dataId=" + dataId + ", group=" + group);
            }

            this.Name = name;
            this.ConfigFilterChainManager = configFilterChainManager;
            this.DataId = dataId;
            this.Group = group;
            this.Tenant = TenantUtil.GetUserTenantForAcm();
            this.IsInitializing = true;
            this.Content = LoadCacheContentFromDiskLocal(name, dataId, group, Tenant);
            this.Md5 = GetMd5String(this.Content);
        }

        public CacheData(ConfigFilterChainManager configFilterChainManager, string name, string dataId, string group, string tenant)
        {
            if (dataId == null || group == null)
            {
                throw new ArgumentNullException("dataId=" + dataId + ", group=" + group);
            }

            this.Name = name;
            this.ConfigFilterChainManager = configFilterChainManager;
            this.DataId = dataId;
            this.Group = group;
            this.Tenant = tenant;
            this.IsInitializing = true;
            this.Content = LoadCacheContentFromDiskLocal(name, dataId, group, Tenant);
            this.Md5 = GetMd5String(this.Content);
            this.IsInitializing = true;
            this.Content = LoadCacheContentFromDiskLocal(name, dataId, group, Tenant);
            this.Md5 = GetMd5String(this.Content);
        }

        public string Name { get; set; }

        public string DataId { get; set; }

        public string Group { get; set; }

        public string Md5 { get; set; }

        public string Content { get; set; }

        public string LastMd5 { get; set; }

        public string Tenant { get; set; }

        public string Type { get; set; }

        public int TaskId { get; set; }

        public bool IsListenSuccess { get; set; }

        public long LastModifiedTs { get; set; }

        public bool IsInitializing { get; set; } = true;

        public bool IsUseLocalConfig { get; set; } = false;

        public long LocalConfigLastModified { get; set; }

        public ConfigFilterChainManager ConfigFilterChainManager { get; set; }

        private List<ManagerListenerWrap> Listeners { get; set; } = new List<ManagerListenerWrap>();

        public bool CheckListenerMd5() => !LastMd5.Equals(Md5);

        public void AddListener(IListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentException("listener is null");
            }

            ManagerListenerWrap wrap = new ManagerListenerWrap(listener, Md5);

            Listeners.Add(wrap);
        }

        public void RemoveListener(IListener listener)
        {
            if (listener == null)
            {
                throw new ArgumentException("listener is null");
            }

            ManagerListenerWrap wrap = new ManagerListenerWrap(listener);
            if (Listeners.Remove(wrap))
            {
                Console.WriteLine($"[{Name}] [remove-listener] ok, dataId={DataId}, group={Group}, cnt={Listeners.Count}");
            }
        }

        public void SetUseLocalConfigInfo(bool useLocalConfigInfo)
        {
            this.IsUseLocalConfig = useLocalConfigInfo;
            if (!useLocalConfigInfo)
            {
                LocalConfigLastModified = -1;
            }
        }

        public void SetLocalConfigInfoVersion(long localConfigLastModified) => this.LocalConfigLastModified = localConfigLastModified;

        public long GetLocalConfigInfoVersion() => LocalConfigLastModified;

        public void SetContent(string content)
        {
            this.Content = content;
            this.Md5 = GetMd5String(this.Content);
        }

        public static string GetMd5String(string config) => (config == null) ? Constants.NULL : HashUtil.GetMd5(config);

        public override string ToString() => $"CacheData [{DataId},{Group}]";

        public override bool Equals(object obj)
        {
            if (WeakReference.Equals(this, obj)) return true;

            return obj is CacheData other
                ? DataId.Equals(other.DataId) && Group.Equals(other.Group)
                : false;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = (prime * result) + ((DataId == null) ? 0 : DataId.GetHashCode());
            result = (prime * result) + ((Group == null) ? 0 : Group.GetHashCode());
            return result;
        }


        public List<IListener> GetListeners()
        {
            var result = new List<IListener>();
            foreach (ManagerListenerWrap wrap in Listeners)
            {
                result.Add(wrap.Listener);
            }

            return result;
        }

        private string LoadCacheContentFromDiskLocal(string name, string dataId, string group, string tenant)
        {
            var content = FileLocalConfigInfoProcessor.GetFailoverAsync(name, dataId, group, tenant)
                .ConfigureAwait(false).GetAwaiter().GetResult();
            content = (content != null)
                ? content
                : FileLocalConfigInfoProcessor.GetSnapshotAync(name, dataId, group, tenant).ConfigureAwait(false).GetAwaiter().GetResult();

            return content;
        }

        internal class ManagerListenerWrap
        {
            internal readonly IListener Listener;
            internal readonly String LastCallMd5 = CacheData.GetMd5String(null);
            internal readonly String LastContent = null;

            internal ManagerListenerWrap(IListener listener)
            {
                this.Listener = listener;
            }

            internal ManagerListenerWrap(IListener listener, String md5)
            {
                this.Listener = listener;
                this.LastCallMd5 = md5;
            }

            internal ManagerListenerWrap(IListener listener, String md5, String lastContent)
            {
                this.Listener = listener;
                this.LastCallMd5 = md5;
                this.LastContent = lastContent;
            }

            public override bool Equals(object obj)
            {
                if (WeakReference.Equals(this, obj)) return true;

                return obj is ManagerListenerWrap other
                    ? Listener.Equals(other.Listener)
                    : false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
