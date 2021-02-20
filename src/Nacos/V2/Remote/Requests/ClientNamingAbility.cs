namespace Nacos.V2.Remote.Requests
{
    public class ClientNamingAbility
    {
        [Newtonsoft.Json.JsonProperty("supportDeltaPush")]
        public bool SupportDeltaPush { get; set; }

        [Newtonsoft.Json.JsonProperty("supportRemoteMetric")]
        public bool SupportRemoteMetric { get; set; }
    }
}
