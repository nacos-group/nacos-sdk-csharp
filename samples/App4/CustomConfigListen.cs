namespace App4
{
    public class CustomConfigListen : Nacos.Config.IListener
    {
        public void ReceiveConfigInfo(string configInfo)
        {
            System.Console.WriteLine("config cb cb cb " + configInfo);
        }
    }
}
