namespace Nacos.V2
{
    public interface IListener
    {
        void ReceiveConfigInfo(string configInfo);
    }
}