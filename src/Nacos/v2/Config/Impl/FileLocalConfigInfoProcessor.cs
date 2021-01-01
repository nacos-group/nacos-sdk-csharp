namespace Nacos.Config.Impl
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public static class FileLocalConfigInfoProcessor
    {
        private static readonly string Failover_Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "nacos", "config");

        private static readonly string Snapshot_Base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "nacos", "config");

        public static async Task<string> GetFailoverAsync(string serverName, string dataId, string group, string tenant)
        {
            FileInfo file = GetFailoverFile(serverName, dataId, group, tenant);

            if (!file.Exists)
            {
                return null;
            }

            using FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] readByte = new byte[fs.Length];
            await fs.ReadAsync(readByte, 0, readByte.Length);
            string readStr = Encoding.UTF8.GetString(readByte);
            fs.Close();
            return readStr;
        }

        private static FileInfo GetFailoverFile(string serverName, string dataId, string group, string tenant)
        {
            string failoverFile;
            failoverFile = Path.Combine(Snapshot_Base, serverName + "_nacos");

            failoverFile = !string.IsNullOrEmpty(tenant)
                ? Path.Combine(failoverFile, "config-data-tenant", tenant, group, dataId)
                : Path.Combine(failoverFile, "config-data", group, dataId);

            var file = new FileInfo(failoverFile);
            return file;
        }

        public static async Task<string> GetSnapshotAync(string name, string dataId, string group, string tenant)
        {
            FileInfo file = GetSnapshotFile(name, dataId, group, tenant);

            if (!file.Exists) return null;

            using FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] readByte = new byte[fs.Length];
            await fs.ReadAsync(readByte, 0, readByte.Length);
            string readStr = Encoding.UTF8.GetString(readByte);
            fs.Close();
            return readStr;
        }

        private static FileInfo GetSnapshotFile(string envName, string dataId, string group, string tenant)
        {
            string snapshotFile;
            snapshotFile = Path.Combine(Snapshot_Base, envName + "_nacos");

            snapshotFile = !string.IsNullOrEmpty(tenant)
                ? Path.Combine(snapshotFile, "snapshot-tenant", tenant, group, dataId)
                : Path.Combine(snapshotFile, "snapshot", group, dataId);

            var file = new FileInfo(snapshotFile);
            return file;
        }

        public static async Task SaveSnapshotAsync(string envName, string dataId, string group, string tenant, string config)
        {
            FileInfo snapshotFile = GetSnapshotFile(envName, dataId, group, tenant);
            if (string.IsNullOrEmpty(config))
            {
                if (snapshotFile.Exists)
                {
                    snapshotFile.Delete();
                }
            }
            else
            {
                if (snapshotFile.Directory != null && !snapshotFile.Directory.Exists)
                {
                    snapshotFile.Directory.Create();
                }

                using FileStream fs = new FileStream(snapshotFile.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                byte[] bytes = Encoding.UTF8.GetBytes(config);
                await fs.WriteAsync(bytes, 0, bytes.Length);
                fs.Close();
            }
        }
    }
}
