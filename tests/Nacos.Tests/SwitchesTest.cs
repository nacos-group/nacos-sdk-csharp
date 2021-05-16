/*namespace Nacos.Tests
{
    using System.Threading.Tasks;
    using Xunit;

    public class SwitchesTest : TestBase
    {
        [Fact]
        public async Task GetSwitches_Should_Succeed()
        {
            var res = await _namingClient.GetSwitchesAsync().ConfigureAwait(false);
            Assert.NotNull(res);
        }

        [Fact]
        public async Task ModifySwitches_Should_Succeed()
        {
            var request = new ModifySwitchesRequest
            {
                 Debug = true,
                 Entry = "test",
                 Value = "test"
            };

            var res = await _namingClient.ModifySwitchesAsync(request).ConfigureAwait(false);
            Assert.True(res);
        }
    }
}
*/