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

        public string LastMd5 { get; set; }

        public string Tenant { get; set; }

        public int TaskId { get; set; }

        public bool IsListenSuccess { get; set; }

        public List<Action<string>> Listeners { get; set; } = new List<Action<string>>();

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
