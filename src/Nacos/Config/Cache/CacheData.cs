namespace Nacos.Config.Cache
{
    using Nacos.Common;
    using Nacos.Config;
    using Nacos.Config.Filter;
    using Nacos.Config.Utils;
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;

    public class CacheData
    {
        public static readonly int PerTaskConfigSize = 3000;

        public CacheData(IConfigFilterChain configFilterChainManager, string name, string dataId, string group)
        {
            if (dataId == null || group == null)
            {
                throw new ArgumentNullException("dataId=" + dataId + ", group=" + group);
            }

            Name = name;
            ConfigFilterChainManager = configFilterChainManager;
            DataId = dataId;
            Group = group;
            Tenant = TenantUtil.GetUserTenantForAcm();
            Listeners = new List<ManagerListenerWrap>();
            IsInitializing = true;
            Content = LoadCacheContentFromDiskLocal(name, dataId, group, Tenant);
            Md5 = GetMd5String(Content);
            EncryptedDataKey = LoadEncryptedDataKeyFromDiskLocal(name, dataId, group, Tenant);
        }

        public CacheData(IConfigFilterChain configFilterChainManager, string name, string dataId, string group, string tenant)
        {
            if (dataId == null || group == null)
            {
                throw new ArgumentNullException("dataId=" + dataId + ", group=" + group);
            }

            Name = name;
            ConfigFilterChainManager = configFilterChainManager;
            DataId = dataId;
            Group = group;
            Tenant = tenant;
            Listeners = new List<ManagerListenerWrap>();
            IsInitializing = true;
            Content = LoadCacheContentFromDiskLocal(name, dataId, group, Tenant);
            Md5 = GetMd5String(Content);
            EncryptedDataKey = LoadEncryptedDataKeyFromDiskLocal(name, dataId, group, Tenant);
        }

        public string Name { get; set; }

        public string DataId { get; set; }

        public string Group { get; set; }

        public string Md5 { get; set; }

        public string Content { get; set; }

        public string Tenant { get; set; }

        public string Type { get; set; }

        public int TaskId { get; set; }

        public bool IsListenSuccess { get; set; }

        public long LastModifiedTs { get; set; }

        public bool IsInitializing { get; set; } = true;

        public bool IsUseLocalConfig { get; set; } = false;

        public long LocalConfigLastModified { get; set; }

        public string EncryptedDataKey { get; set; }

        public IConfigFilterChain ConfigFilterChainManager { get; set; }

        private List<ManagerListenerWrap> Listeners { get; set; }

        public void CheckListenerMd5()
        {
            foreach (var wrap in Listeners)
            {
                if (!wrap.LastCallMd5.Equals(Md5))
                {
                    SafeNotifyListener(DataId, Group, Content, Type, Md5, EncryptedDataKey, wrap);
                }
            }
        }

        private void SafeNotifyListener(string dataId, string group, string content, string type,
            string md5, string encryptedDataKey, ManagerListenerWrap wrap)
        {
            var listener = wrap.Listener;

            ConfigResponse cr = new ConfigResponse();
            cr.SetDataId(dataId);
            cr.SetGroup(group);
            cr.SetContent(content);
            cr.SetEncryptedDataKey(encryptedDataKey);
            ConfigFilterChainManager.DoFilter(null, cr);

            // after filter, such as decrypted value
            string contentTmp = cr.GetContent();

            wrap.LastContent = content;
            wrap.LastCallMd5 = md5;

            // should pass the value after filter
            listener.ReceiveConfigInfo(contentTmp);
        }

        public void AddListener(IListener listener)
        {
            if (listener == null) throw new ArgumentException("listener is null");

            ManagerListenerWrap wrap = new ManagerListenerWrap(listener, Md5, Content);

            Listeners.Add(wrap);
        }

        public void RemoveListener(IListener listener)
        {
            if (listener == null) throw new ArgumentException("listener is null");

            ManagerListenerWrap wrap = new ManagerListenerWrap(listener);
            if (Listeners.Remove(wrap))
            {
                Console.WriteLine($"[{Name}] [remove-listener] ok, dataId={DataId}, group={Group}, cnt={Listeners.Count}");
            }
        }

        public void SetUseLocalConfigInfo(bool useLocalConfigInfo)
        {
            IsUseLocalConfig = useLocalConfigInfo;
            if (!useLocalConfigInfo)
            {
                LocalConfigLastModified = -1;
            }
        }

        public void SetLocalConfigInfoVersion(long localConfigLastModified) => LocalConfigLastModified = localConfigLastModified;

        public long GetLocalConfigInfoVersion() => LocalConfigLastModified;

        public void SetContent(string content)
        {
            Content = content;
            Md5 = GetMd5String(Content);
        }

        public static string GetMd5String(string config) => config == null ? Constants.NULL : HashUtil.GetMd5(config);

        public override string ToString() => $"CacheData [{DataId},{Group}]";

        public override bool Equals(object obj) => obj is CacheData other && DataId.Equals(other.DataId) && Group.Equals(other.Group);

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = (prime * result) + (DataId == null ? 0 : DataId.GetHashCode());
            result = (prime * result) + (Group == null ? 0 : Group.GetHashCode());
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
            content = content != null
                ? content
                : FileLocalConfigInfoProcessor.GetSnapshotAsync(name, dataId, group, tenant).ConfigureAwait(false).GetAwaiter().GetResult();

            return content;
        }

        private string LoadEncryptedDataKeyFromDiskLocal(string name, string dataId, string group, string tenant)
        {
            var encryptedDataKey = FileLocalConfigInfoProcessor.GetEncryptDataKeyFailoverAsync(name, dataId, group, tenant).ConfigureAwait(false).GetAwaiter().GetResult();

            encryptedDataKey = encryptedDataKey != null
                ? encryptedDataKey
                : FileLocalConfigInfoProcessor.GetEncryptDataKeySnapshotAsync(name, dataId, group, tenant).ConfigureAwait(false).GetAwaiter().GetResult();

            return encryptedDataKey;
        }

        internal class ManagerListenerWrap
        {
            internal IListener Listener;
            internal string LastCallMd5 = GetMd5String(null);
            internal string LastContent = null;

            internal ManagerListenerWrap(IListener listener)
            {
                Listener = listener;
            }

            internal ManagerListenerWrap(IListener listener, string md5, string lastContent)
            {
                Listener = listener;
                LastCallMd5 = md5;
                LastContent = lastContent;
            }

            public override bool Equals(object obj) => obj is ManagerListenerWrap other && Listener.Equals(other.Listener);

            public override int GetHashCode() => base.GetHashCode();
        }
    }
}
