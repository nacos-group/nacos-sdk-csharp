namespace Nacos.Tests.Config.Impl
{
    using Google.Protobuf;
    using Nacos.Config.Impl;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class FileLocalConfigInfoProcessorTests
    {
        private const string SERVER_NAME = "config_local_test";
        private const string DATA_ID = "test";
        private const string GROUP = "g";
        private const string TENANT = "cs";

        [Fact]
        public async Task Get_Failover_Should_Succeed()
        {
            // Manually write failover files
            var config = @"this is config content";
            await WriteToTestFailoverFileAsync(TENANT, false, config).ConfigureAwait(false);

            var getConfig = await FileLocalConfigInfoProcessor.GetFailoverAsync(SERVER_NAME, DATA_ID, GROUP, TENANT).ConfigureAwait(false);

            Assert.Equal(config, getConfig);
        }

        [Fact]
        public async Task Get_Failover_Should_No_Tenant_Succeed()
        {
            var config = @"this is config content and no tenant";
            await WriteToTestFailoverFileAsync(string.Empty, false, config).ConfigureAwait(false);

            var getConfig1 = await FileLocalConfigInfoProcessor.GetFailoverAsync(SERVER_NAME, DATA_ID, GROUP, string.Empty).ConfigureAwait(false);

            Assert.Equal(config, getConfig1);
        }

        [Fact]
        public async Task Get_Encrypt_Data_Key_Failover_Should_Succeed()
        {
            var config = @"this is encrypt data key config content";
            await WriteToTestFailoverFileAsync(TENANT, true, config).ConfigureAwait(false);

            var getConfig = await FileLocalConfigInfoProcessor.GetEncryptDataKeyFailoverAsync(SERVER_NAME, DATA_ID, GROUP, TENANT).ConfigureAwait(false);

            Assert.Equal(config, getConfig);
        }

        [Fact]
        public async Task Get_Encrypt_Data_Key_Failover_No_Tenant_Should_Succeed()
        {
            var config = @"this is encrypt data key config content and no tenant";
            await WriteToTestFailoverFileAsync(string.Empty, true, config).ConfigureAwait(false);

            var getConfig = await FileLocalConfigInfoProcessor.GetEncryptDataKeyFailoverAsync(SERVER_NAME, DATA_ID, GROUP, string.Empty).ConfigureAwait(false);

            Assert.Equal(config, getConfig);
        }

        [Fact]
        public async Task Save_And_Get_Snapshot_Should_Succeed()
        {
            var config = @"this is config content";
            await FileLocalConfigInfoProcessor.SaveSnapshotAsync(SERVER_NAME, DATA_ID, GROUP, TENANT, config).ConfigureAwait(false);

            var getConfig = await FileLocalConfigInfoProcessor.GetSnapshotAsync(SERVER_NAME, DATA_ID, GROUP, TENANT).ConfigureAwait(false);

            Assert.Equal(config, getConfig);
        }

        [Fact]
        public async Task Save_And_Get_Snapshot_No_Tenant_Should_Succeed()
        {
            var config = @"this is config content and no tenant";
            await FileLocalConfigInfoProcessor.SaveSnapshotAsync(SERVER_NAME, DATA_ID, GROUP, string.Empty, config).ConfigureAwait(false);

            var getConfig = await FileLocalConfigInfoProcessor.GetSnapshotAsync(SERVER_NAME, DATA_ID, GROUP, string.Empty).ConfigureAwait(false);

            Assert.Equal(config, getConfig);
        }

        [Fact]
        public async Task Save_And_Get_Encrypt_Data_Key_Snapshot_Should_Succeed()
        {
            var config = @"this is encrypt data key config content";
            await FileLocalConfigInfoProcessor.SaveEncryptDataKeySnapshotAsync(SERVER_NAME, DATA_ID, GROUP, TENANT, config).ConfigureAwait(false);

            var getConfig = await FileLocalConfigInfoProcessor.GetEncryptDataKeySnapshotAsync(SERVER_NAME, DATA_ID, GROUP, TENANT).ConfigureAwait(false);

            Assert.Equal(config, getConfig);
        }

        [Fact]
        public async Task Save_And_Get_Encrypt_Data_Key_Snapshot_No_Tenant_Should_Succeed()
        {
            var config = @"this is encrypt data key config content and no tenant";
            await FileLocalConfigInfoProcessor.SaveEncryptDataKeySnapshotAsync(SERVER_NAME, DATA_ID, GROUP, string.Empty, config).ConfigureAwait(false);

            var getConfig = await FileLocalConfigInfoProcessor.GetEncryptDataKeySnapshotAsync(SERVER_NAME, DATA_ID, GROUP, string.Empty).ConfigureAwait(false);

            Assert.Equal(config, getConfig);
        }

        private async Task WriteToTestFailoverFileAsync(string tenant, bool isEncrypted, string config)
        {
            var basePath = Nacos.Utils.EnvUtil.GetEnvValue("JM.SNAPSHOT.PATH", Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            var path = Path.Combine(basePath, "nacos", "config");
            var failoverFile = Path.Combine(path, SERVER_NAME + "_nacos", isEncrypted ? "encrypted-data-key" : string.Empty);
            if (isEncrypted)
            {
                failoverFile = !string.IsNullOrEmpty(tenant)
                ? Path.Combine(failoverFile, "failover-tenant", tenant, GROUP, DATA_ID)
                : Path.Combine(failoverFile, "failover", GROUP, DATA_ID);
            }
            else
            {
                failoverFile = !string.IsNullOrEmpty(tenant)
                    ? Path.Combine(failoverFile, "config-data-tenant", tenant, GROUP, DATA_ID)
                    : Path.Combine(failoverFile, "config-data", GROUP, DATA_ID);
            }

            var file = new FileInfo(failoverFile);
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
}
