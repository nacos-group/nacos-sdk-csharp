namespace Nacos.V2.Config.Impl
{
    using Nacos.V2.Common;
    using Nacos.V2.Config.FilterImpl;
    using Nacos.V2.Utils;
    using System;
    using System.Collections.Generic;

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

        public int TaskId { get; set; }

        public bool IsListenSuccess { get; set; }

        public long LastModifiedTs { get; set; }

        public bool IsInitializing { get; set; } = true;

        public bool IsUseLocalConfig { get; set; } = false;

        public long LocalConfigLastModified { get; set; }

        public ConfigFilterChainManager ConfigFilterChainManager { get; set; }

        public List<Action<string>> Listeners { get; set; } = new List<Action<string>>();

        public bool CheckListenerMd5() => !LastMd5.Equals(Md5);

        public void AddListener(Action<string> action)
        {
            Listeners.Add(action);
        }

        public void RemoveListener(Action<string> action)
        {
            Listeners.Remove(action);
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

        private string LoadCacheContentFromDiskLocal(string name, string dataId, string group, string tenant)
        {
            var content = FileLocalConfigInfoProcessor.GetFailoverAsync(name, dataId, group, tenant)
                .ConfigureAwait(false).GetAwaiter().GetResult();
            content = (content != null)
                ? content
                : FileLocalConfigInfoProcessor.GetSnapshotAync(name, dataId, group, tenant).ConfigureAwait(false).GetAwaiter().GetResult();

            return content;
        }
    }
}
