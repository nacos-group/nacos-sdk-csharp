namespace Nacos.V2.Remote
{
    internal class ConnectionEvent
    {
        public static int CONNECTED = 1;

        public static int DISCONNECTED = 0;

        public int EventType { get; set; }

        public ConnectionEvent(int eventType) => EventType = eventType;

        public bool IsConnected() => EventType == CONNECTED;

        public bool IsDisConnected() => EventType == DISCONNECTED;
    }
}
