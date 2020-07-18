namespace Nacos.Tests
{
    using System.Threading.Tasks;
    using Xunit;

    public class ClusterTest : TestBase
    {
        [Fact]
        public async Task ListClusterServers_Should_Succeed()
        {
            var request = new ListClusterServersRequest
            { };

            var res = await _namingClient.ListClusterServersAsync(request);
            Assert.NotNull(res);
        }

        [Fact]
        public async Task GetCurrentClusterLeader_Should_Succeed()
        {
            var res = await _namingClient.GetCurrentClusterLeaderAsync();
            Assert.NotNull(res);
        }
    }
}
