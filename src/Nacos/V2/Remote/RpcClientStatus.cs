namespace Nacos.V2.Remote
{
    public class RpcClientStatus
    {
        /// <summary>
        /// wait to  init serverlist factory...
        /// </summary>
        public const int WAIT_INIT = 0;

        /// <summary>
        /// server list factory is ready,wait to start
        /// </summary>
        public const int INITIALIZED = 1;

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

        internal const string WAIT_INIT_NAME = "WAIT_INIT";
        internal const string INITIALIZED_NAME = "INITIALIZED";
        internal const string STARTING_NAME = "STARTING";
        internal const string RUNNING_NAME = "RUNNING";
        internal const string UNHEALTHY_NAME = "UNHEALTHY";
        internal const string SHUTDOWN_NAME = "SHUTDOWN";

        public static string GetStatusName(int status)
        {
            var res = string.Empty;

            switch (status)
            {
                case 0:
                    res = WAIT_INIT_NAME;
                    break;
                case 1:
                    res = INITIALIZED_NAME;
                    break;
                case 2:
                    res = STARTING_NAME;
                    break;
                case 3:
                    res = UNHEALTHY_NAME;
                    break;
                case 4:
                    res = RUNNING_NAME;
                    break;
                case 5:
                    res = SHUTDOWN_NAME;
                    break;
                default:
                    break;
            }

            return res;
        }
    }
}
