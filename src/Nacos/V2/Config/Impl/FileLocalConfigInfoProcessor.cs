namespace Nacos.V2.Config.Impl
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public static class FileLocalConfigInfoProcessor
    {
        private static readonly string LOCAL_SNAPSHOT_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "nacos", "config");

        public static async Task<string> GetFailoverAsync(string serverName, string dataId, string group, string tenant)
        {
            FileInfo file = GetFailoverFile(serverName, dataId, group, tenant);

            if (!file.Exists) return null;

            try
            {
                return await ReadFileAsync(file).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                return null;
            }
        }

        internal static FileInfo GetFailoverFile(string serverName, string dataId, string group, string tenant)
        {
            string failoverFile;
            failoverFile = Path.Combine(LOCAL_SNAPSHOT_PATH, serverName + "_nacos");

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

            try
            {
                return await ReadFileAsync(file).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex);
                return null;
            }
        }

        private static async Task<string> ReadFileAsync(FileInfo file)
        {
            using FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] readByte = new byte[fs.Length];
            await fs.ReadAsync(readByte, 0, readByte.Length).ConfigureAwait(false);
            string readStr = Encoding.UTF8.GetString(readByte);
            fs.Close();
            return readStr;
        }

        private static FileInfo GetSnapshotFile(string envName, string dataId, string group, string tenant)
        {
            string snapshotFile;
            snapshotFile = Path.Combine(LOCAL_SNAPSHOT_PATH, envName + "_nacos");

            snapshotFile = !string.IsNullOrEmpty(tenant)
                ? Path.Combine(snapshotFile, "snapshot-tenant", tenant, group, dataId)
                : Path.Combine(snapshotFile, "snapshot", group, dataId);

            var file = new FileInfo(snapshotFile);
            return file;
        }

        public static async Task SaveSnapshotAsync(string envName, string dataId, string group, string tenant, string config)
        {
            FileInfo file = GetSnapshotFile(envName, dataId, group, tenant);
            if (string.IsNullOrEmpty(config))
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex);
                }
            }
            else
            {
                try
                {
                    if (file.Directory != null && !file.Directory.Exists)
                    {
                        file.Directory.Create();
                    }

                    using FileStream fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    byte[] bytes = Encoding.UTF8.GetBytes(config);
                    fs.SetLength(bytes.Length);
                    await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                    fs.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// 获取容灾配置的 EncryptedDataKey。NULL表示没有本地文件或抛出异常.
        /// </summary>
        /// <param name="envName">envName</param>
        /// <param name="dataId">dataId</param>
        /// <param name="group">group</param>
        /// <param name="tenant">tenant</param>
        /// <returns>EncryptedDataKey</returns>
        public static async Task<string> GetEncryptDataKeyFailover(string envName, string dataId, string group, string tenant)
        {
            FileInfo file = GetEncryptDataKeyFailoverFile(envName, dataId, group, tenant);

            if (!file.Exists) return null;

            try
            {
                return await ReadFileAsync(file).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        private static FileInfo GetEncryptDataKeyFailoverFile(string envName, string dataId, string group, string tenant)
        {
            string snapshotFile;
            snapshotFile = Path.Combine(LOCAL_SNAPSHOT_PATH, envName + "_nacos", "encrypted-data-key");

            snapshotFile = !string.IsNullOrEmpty(tenant)
                ? Path.Combine(snapshotFile, "failover-tenant", tenant, group, dataId)
                : Path.Combine(snapshotFile, "failover", group, dataId);

            var file = new FileInfo(snapshotFile);
            return file;
        }

        private static FileInfo GetEncryptDataKeySnapshotFile(string envName, string dataId, string group, string tenant)
        {
            string snapshotFile;
            snapshotFile = Path.Combine(LOCAL_SNAPSHOT_PATH, envName + "_nacos", "encrypted-data-key");

            snapshotFile = !string.IsNullOrEmpty(tenant)
                ? Path.Combine(snapshotFile, "snapshot-tenant", tenant, group, dataId)
                : Path.Combine(snapshotFile, "snapshot", group, dataId);

            var file = new FileInfo(snapshotFile);
            return file;
        }

        /// <summary>
        /// 保存 encryptDataKey 的snapshot。如果内容为NULL，则删除snapshot.
        /// </summary>
        /// <param name="envName">envName</param>
        /// <param name="dataId">dataId</param>
        /// <param name="group">group</param>
        /// <param name="tenant">tenant</param>
        /// <param name="encryptDataKey">encryptDataKey</param>
        /// <returns>Task</returns>
        public static async Task SaveEncryptDataKeySnapshot(string envName, string dataId, string group, string tenant, string encryptDataKey)
        {
            /*if (!SnapShotSwitch.getIsSnapShot())
            {
                return;
            }*/

            var file = GetEncryptDataKeySnapshotFile(envName, dataId, group, tenant);
            try
            {
                if (encryptDataKey == null)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                    }
                }
                else
                {
                    if (file.Directory != null && !file.Directory.Exists)
                    {
                        file.Directory.Create();
                    }

                    using FileStream fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    byte[] bytes = Encoding.UTF8.GetBytes(encryptDataKey);
                    fs.SetLength(bytes.Length);
                    await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                    fs.Close();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 获取本地缓存文件的 EncryptedDataKey。NULL表示没有本地文件或抛出异常.
        /// </summary>
        /// <param name="envName">envName</param>
        /// <param name="dataId">dataId</param>
        /// <param name="group">group</param>
        /// <param name="tenant">tenant</param>
        /// <returns>EncryptedDataKey</returns>
        public static async Task<string> GetEncryptDataKeySnapshot(string envName, string dataId, string group, string tenant)
        {
            /*if (!SnapShotSwitch.getIsSnapShot())
            {
                return null;
            }*/

            FileInfo file = GetEncryptDataKeySnapshotFile(envName, dataId, group, tenant);
            if (!file.Exists) return null;

            try
            {
                return await ReadFileAsync(file).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }
    }
}
