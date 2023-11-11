namespace Nacos.Tests.Config.Utils
{
    using Nacos.Config.Utils;
    using Xunit;

    public class SnapShotSwitchTest
    {
        [Fact]
        public void Snap_Shot_Switch_Set_Get_Should_Successed()
        {
            SnapShotSwitch.SetIsSnapShot(true);
            Assert.True(SnapShotSwitch.GetIsSnapShot());
        }
    }
}
