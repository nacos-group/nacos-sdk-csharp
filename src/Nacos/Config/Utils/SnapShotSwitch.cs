namespace Nacos.Config.Utils
{
    using Nacos.Common;
    using System;

    public class SnapShotSwitch
    {
        private static bool isSnapShot = true;

        public static bool GetIsSnapShot() => isSnapShot;

        public static void SetIsSnapShot(bool isSnapShot)
        {
            SnapShotSwitch.isSnapShot = isSnapShot;

            // LocalConfigInfoProcessor.cleanAllSnapshot();
        }
    }
}
