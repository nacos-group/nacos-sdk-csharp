namespace Nacos.Remote.Requests
{
    using System.Collections.Generic;
    using Nacos.Remote;

    public class ConnectionSetupRequest : CommonRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("clientVersion")]
        public string ClientVersion { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("abilities")]
        public ClientAbilities Abilities { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tenant")]
        public string Tenant { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("labels")]
        public Dictionary<string, string> Labels = new Dictionary<string, string>();

        public override string GetRemoteType() => RemoteRequestType.Req_ConnectionSetup;
    }
}
