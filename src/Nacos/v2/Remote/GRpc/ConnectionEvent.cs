namespace Nacos.V2.Remote.GRpc
{
    public class ConnectionEvent
    {
        public static readonly int CONNECTED = 1;

        public static readonly int DISCONNECTED = 0;

        public ConnectionEvent(int eventType)
        {
            this.EventType = eventType;
        }

        public int EventType { get; private set; }

        public bool IsConnected() => EventType.Equals(CONNECTED);

        public bool IsDisConnected() => EventType.Equals(DISCONNECTED);
    }
}
