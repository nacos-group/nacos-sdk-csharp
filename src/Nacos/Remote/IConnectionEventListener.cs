namespace Nacos.Remote
{
    public interface IConnectionEventListener
    {
        void OnConnected();

        void OnDisConnected();
    }
}
