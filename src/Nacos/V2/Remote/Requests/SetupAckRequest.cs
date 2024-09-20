namespace Nacos.V2.Remote.Requests
{
    using System.Collections.Generic;

    public class SetupAckRequest : CommonRequest
    {
        public Dictionary<string, bool> AbilityTable { get; set; }

        public override string GetRemoteType() => RemoteRequestType.Req_SetupAck;
    }
}
