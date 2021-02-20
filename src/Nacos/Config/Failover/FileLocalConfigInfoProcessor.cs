namespace Nacos
{
    using System.IO;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public class FileLocalConfigInfoProcessor : ILocalConfigInfoProcessor
    {
        private readonly string failover_base = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "nacos", "config");

        private readonly string snapshot_base = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "nacos", "config");

        public async Task<string> GetFailoverAsync(string serverName, string dataId, string group, string tenant)
        {
            FileInfo file = GetFailoverFile(serverName, dataId, group, tenant);

            if (!file.Exists)
            {
                return null;
            }

            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] readByte = new byte[fs.Length];
                await fs.ReadAsync(readByte, 0, readByte.Length);
                string readStr = Encoding.UTF8.GetString(readByte);
                fs.Close();
                return readStr;
            }
        }

        private FileInfo GetFailoverFile(string serverName, string dataId, string group, string tenant)
        {
            string failoverFile;
            failoverFile = Path.Combine(snapshot_base, serverName + "_nacos");
            if (!string.IsNullOrEmpty(tenant))
            {
                failoverFile = Path.Combine(failoverFile, "config-data-tenant", tenant, group, dataId);
            }
            else
            {
                failoverFile = Path.Combine(failoverFile, "config-data", group, dataId);
            }

            var file = new FileInfo(failoverFile);
            return file;
        }

        public async Task<string> GetSnapshotAync(string name, string dataId, string group, string tenant)
        {
            FileInfo file = GetSnapshotFile(name, dataId, group, tenant);

            if (!file.Exists)
            {
                return null;
            }

            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] readByte = new byte[fs.Length];
                await fs.ReadAsync(readByte, 0, readByte.Length);
                string readStr = Encoding.UTF8.GetString(readByte);
                fs.Close();
                return readStr;
            }
        }

        private FileInfo GetSnapshotFile(string envName, string dataId, string group, string tenant)
        {
            string snapshotFile;
            snapshotFile = Path.Combine(snapshot_base, envName + "_nacos");
            if (!string.IsNullOrEmpty(tenant))
            {
                snapshotFile = Path.Combine(snapshotFile, "snapshot-tenant", tenant, group, dataId);
            }
            else
            {
                snapshotFile = Path.Combine(snapshotFile, "snapshot", group, dataId);
            }

            var file = new FileInfo(snapshotFile);
            return file;
        }

        public async Task SaveSnapshotAsync(string envName, string dataId, string group, string tenant, string config)
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

                using (FileStream fs = new FileStream(snapshotFile.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(config);
                    fs.SetLength(bytes.Length);
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                    fs.Close();
                }
            }
        }
    }
}
