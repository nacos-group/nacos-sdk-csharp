namespace Nacos.Tests.Base
{
    public class TestConfigListen : Nacos.Config.IListener
    {
        public void ReceiveConfigInfo(string configInfo)
        {
            System.Console.WriteLine("config cs " + configInfo);
        }
    }
}
