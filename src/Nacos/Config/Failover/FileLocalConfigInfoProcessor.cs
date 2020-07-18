namespace Nacos
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class FileLocalConfigInfoProcessor : ILocalConfigInfoProcessor
    {
        private readonly string _failover_base = Path.Combine(Directory.GetCurrentDirectory(), "nacos-data", "data");
        private readonly string _snapshot_base = Path.Combine(Directory.GetCurrentDirectory(), "nacos-data", "snapshot");

        public async Task<string> GetFailoverAsync(string dataId, string group, string tenant)
        {
            string failoverFile;
            if (!string.IsNullOrEmpty(tenant))
            {
                failoverFile = Path.Combine(_snapshot_base, "config-data-tenant", tenant, group);
            }
            else
            {
                failoverFile = Path.Combine(_snapshot_base, "config-data", group);
            }

            var file = new FileInfo(failoverFile + dataId);

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

        public async Task<string> GetSnapshotAync(string dataId, string group, string tenant)
        {
            FileInfo file = GetSnapshotFile(dataId, group, tenant);

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

        private FileInfo GetSnapshotFile(string dataId, string group, string tenant)
        {
            string snapshotFile;
            if (!string.IsNullOrEmpty(tenant))
            {
                snapshotFile = Path.Combine(_snapshot_base, "snapshot-tenant", tenant, group);
            }
            else
            {
                snapshotFile = Path.Combine(_snapshot_base, "snapshot", group);
            }

            var file = new FileInfo(snapshotFile + dataId);
            return file;
        }

        public async Task SaveSnapshotAsync(string dataId, string group, string tenant, string config)
        {
            FileInfo snapshotFile = GetSnapshotFile(dataId, group, tenant);
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
                    await fs.WriteAsync(bytes, 0, bytes.Length);
                    fs.Close();
                }
            }
        }
    }
}
