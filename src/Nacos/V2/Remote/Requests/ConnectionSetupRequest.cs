namespace Nacos.V2.Remote.Requests
{
    using System.Collections.Generic;

    public class ConnectionSetupRequest : CommonRequest
    {
        [Newtonsoft.Json.JsonProperty("clientVersion")]
        public string ClientVersion { get; set; }

        [Newtonsoft.Json.JsonProperty("abilities")]
        public ClientAbilities Abilities { get; set; }

        [Newtonsoft.Json.JsonProperty("tenant")]
        public string Tenant { get; set; }

        [Newtonsoft.Json.JsonProperty("labels")]
        public Dictionary<string, string> Labels = new Dictionary<string, string>();

        public override string GetRemoteType() => RemoteRequestType.Req_ConnectionSetup;
    }
}
