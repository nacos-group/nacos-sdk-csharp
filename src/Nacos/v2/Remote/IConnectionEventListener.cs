namespace Nacos.V2.Remote
{
    public interface IConnectionEventListener
    {
        void OnConnected();

        void OnDisConnected();
    }
}
