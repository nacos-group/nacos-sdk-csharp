namespace Nacos.V2.Remote
{
    public class RemoteServerInfo
    {
        public string ServerIp { get; set; }

        public int ServerPort { get; set; }

        public string GetAddress() => $"{ServerIp}{V2.Common.Constants.COLON}{ServerPort}";
    }
}
