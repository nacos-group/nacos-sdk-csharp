namespace Nacos.Config
{
    public interface IListener
    {
        void ReceiveConfigInfo(string configInfo);
    }
}
