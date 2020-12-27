namespace Nacos.Config.Impl
{
    using System;
    using System.Collections.Generic;

    public class CacheData
    {
        public static readonly int PerTaskConfigSize = 3000;

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

        public List<Action<string>> Listeners { get; set; } = new List<Action<string>>();

        public void SetContent(string content)
        {
            this.LastMd5 = new System.Text.StringBuilder(64).Append(this.Md5 ?? Nacos.Utilities.HashUtil.GetMd5("")).ToString();
            this.Content = content;
            this.Md5 = Nacos.Utilities.HashUtil.GetMd5(this.Content);
        }

        public bool CheckListenerMd5() => !LastMd5.Equals(Md5);

        public void AddListener(Action<string> action)
        {
            Listeners.Add(action);
        }

        public void RemoveListener(Action<string> action)
        {
            Listeners.Remove(action);
        }
    }
}
