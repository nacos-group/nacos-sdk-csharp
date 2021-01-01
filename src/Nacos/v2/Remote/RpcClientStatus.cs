namespace Nacos.Remote
{
    using Microsoft.Extensions.Logging;
    using Nacos.Utilities;
    using System;
    using System.Collections.Generic;

    public class RpcClientStatus
    {
        /// <summary>
        /// wait to  init serverlist factory...
        /// </summary>
        public const int WAIT_INIT = 0;

        /// <summary>
        /// server list factory is ready,wait to start
        /// </summary>
        public const int INITED = 1;

        /// <summary>
        /// server list factory is ready,wait to start
        /// </summary>
        public const int STARTING = 2;

        /// <summary>
        /// client is running...
        /// </summary>
        public const int RUNNING = 4;

        /// <summary>
        /// client unhealthy,may closed by server,in rereconnecting
        /// </summary>
        public const int UNHEALTHY = 3;

        /// <summary>
        /// client is shutdown...
        /// </summary>
        public const int SHUTDOWN = 5;

        public int Status { get; set; }

        public string Desc { get; set; }

        public RpcClientStatus(int status)
        {
            this.Status = status;
        }
    }
}
