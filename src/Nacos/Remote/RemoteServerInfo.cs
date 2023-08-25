namespace Nacos.Remote
{
    public class RemoteServerInfo
    {
        public string ServerIp { get; set; }

        public int ServerPort { get; set; }


/* 项目“Nacos (netstandard2.0)”的未合并的更改
在此之前:
        public string GetAddress() => $"{ServerIp}{V2.Common.Constants.COLON}{ServerPort}";
在此之后:
        public string GetAddress() => $"{ServerIp}{Constants.COLON}{ServerPort}";
*/
        public string GetAddress() => $"{ServerIp}{Nacos.Common.Constants.COLON}{ServerPort}";
    }
}
