namespace Nacos.V2.Remote
{
    internal class ReconnectContext
    {
        public ReconnectContext(RemoteServerInfo serverInfo, bool onRequestFail)
        {
            this.OnRequestFail = onRequestFail;
            this.ServerInfo = serverInfo;
        }

        public bool OnRequestFail { get; set; }

        public RemoteServerInfo ServerInfo { get; set; }
    }
}
