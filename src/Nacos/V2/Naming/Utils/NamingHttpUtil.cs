namespace Nacos.V2.Naming.Utils
{
    using System;
    using Nacos.V2.Common;

    public class NamingHttpUtil
    {
        public static System.Collections.Generic.Dictionary<string, string> BuildHeader()
        {
            return new System.Collections.Generic.Dictionary<string, string>
            {
                { HttpHeaderConsts.CLIENT_VERSION_HEADER, Constants.CLIENT_VERSION },
                { HttpHeaderConsts.USER_AGENT_HEADER, Constants.CLIENT_VERSION },
                { HttpHeaderConsts.ACCEPT_ENCODING, "gzip,deflate,sdch" },
                { HttpHeaderConsts.CONNECTION, "Keep-Alive" },
                { HttpHeaderConsts.REQUEST_ID, Guid.NewGuid().ToString("N") },
                { HttpHeaderConsts.REQUEST_MODULE, "Naming" },
            };
        }
    }
}
