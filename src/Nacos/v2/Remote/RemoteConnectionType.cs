namespace Nacos.Remote
{
    using Microsoft.Extensions.Logging;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RemoteConnectionType
    {
        public static string RSOCKET = "RSOCKET";

        public static string TB_REMOTEING = "TB_REMOTEING";

        public static string GRPC = "GRPC";

        public string Type { get; set; }

        public RemoteConnectionType(string type) => this.Type = type;
    }
}
