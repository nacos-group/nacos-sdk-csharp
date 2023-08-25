namespace Nacos.Remote
{
    internal class ReconnectContext
    {
        public ReconnectContext(RemoteServerInfo serverInfo, bool onRequestFail)
        {
            OnRequestFail = onRequestFail;
            ServerInfo = serverInfo;
        }

        public bool OnRequestFail { get; set; }

        public RemoteServerInfo ServerInfo { get; set; }
    }
}
