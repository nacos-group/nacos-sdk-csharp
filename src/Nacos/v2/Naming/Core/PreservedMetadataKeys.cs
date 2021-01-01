namespace Nacos.Naming.Core
{
    using Nacos.Remote;
    using System.Collections.Generic;

    public class PreservedMetadataKeys
    {
        public static string REGISTER_SOURCE = "preserved.register.source";

        public static string HEART_BEAT_TIMEOUT = "preserved.heart.beat.timeout";

        public static string IP_DELETE_TIMEOUT = "preserved.ip.delete.timeout";

        public static string HEART_BEAT_INTERVAL = "preserved.heart.beat.interval";

        public static string INSTANCE_ID_GENERATOR = "preserved.instance.id.generator";
    }
}
