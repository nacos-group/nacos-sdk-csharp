namespace Nacos.Tests.Naming.Backups
{
    using Nacos.Naming.Backups;
    using Nacos.Naming.Cache;
    using Nacos.Naming.Dtos;
    using Nacos.Naming.Event;
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;
    using static Nacos.Tests.Naming.Cache.ServiceInfoHolderTests;

    public class FailoverReactorTest
    {
        private static readonly string FILE_PATH_NACOS = "nacos";
        private static readonly string FILE_PATH_NAMING = "naming";
        private string @namespace = string.Empty;
        private ServiceInfo serviceInfo;
        private FailoverReactor failoverReactor;

        public FailoverReactorTest()
        {
            InstancesChangeNotifier notifier = new InstancesChangeNotifier();
            serviceInfo = new ServiceInfo
            {
                Name = "My_GROUP@@mysvc2",
                Clusters = "",
                CacheMillis = 10000,
                Hosts = new List<Instance>
                {
                    new Instance
                    {
                       InstanceId = "127.0.0.1#9563#DEFAULT#My_GROUP@@mysvc2",
                       Ip = "127.0.0.1",
                       Port = 9563,
                       Weight = 1,
                       Healthy = true,
                       Enabled = true,
                       Ephemeral = true,
                       ClusterName = "DEFAULT",
                       ServiceName = "My_GROUP@@mysvc2",
                       Metadata = new Dictionary<string, string>()
                    }
                },
                LastRefTime = 1633588038730,
                Checksum = "",
                AllIPs = false,
            };
            NacosSdkOptions options = new NacosSdkOptions() { };
            var holder = new ServiceInfoHolder(@namespace, options, notifier);

            holder.ProcessServiceInfo(serviceInfo);
            var cacheDir = GetCacheDir(@namespace);
            failoverReactor = new FailoverReactor(holder, cacheDir);
        }

        [Fact]
        public async Task RunUpdateBackupFile_Should_Succeed()
        {
            await failoverReactor.RunUpdateBackupFile().ConfigureAwait(false);
            await failoverReactor.RunFailoverFileRead().ConfigureAwait(false);
            var getServiceInfo = failoverReactor.GetService(serviceInfo.Name);

            Assert.NotNull(getServiceInfo);
            Assert.Equal(serviceInfo.Name, getServiceInfo.Name);
            Assert.NotEmpty(getServiceInfo.Hosts);
        }

        //[Fact]
        //public async Task RunSwitchRefresh_Should_Succeed()
        //{
        //    var path = GetCacheDir(@namespace);
        //    var switchFile = new FileInfo(Path.Combine(path, "failover", "00-00---000-VIPSRV_FAILOVER_SWITCH-000---00-00"));
        //    await WriteAsync("1", switchFile.FullName).ConfigureAwait(false);
        //    await Task.Delay(5500).ConfigureAwait(false);
        //    Assert.True(failoverReactor.IsFailoverSwitch());

        //    await WriteAsync("0", switchFile.FullName).ConfigureAwait(false);
        //    await Task.Delay(5500).ConfigureAwait(false);
        //    Assert.False(failoverReactor.IsFailoverSwitch());
        //}

        private string GetCacheDir(string @namespace)
        {
            var jmSnapshotPath = EnvUtil.GetEnvValue("JM.SNAPSHOT.PATH");
            var cacheDir = string.Empty;
            if (!string.IsNullOrWhiteSpace(jmSnapshotPath))
            {
                cacheDir = Path.Combine(jmSnapshotPath, FILE_PATH_NACOS, FILE_PATH_NAMING, @namespace);
            }
            else
            {
                cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), FILE_PATH_NACOS, FILE_PATH_NAMING, @namespace);
            }

            return cacheDir;
        }

        private async Task WriteAsync(string content, string path)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                fs.SetLength(bytes.Length);
                await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
