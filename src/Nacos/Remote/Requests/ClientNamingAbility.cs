namespace Nacos.Remote.Requests
{
    public class ClientNamingAbility
    {
        [System.Text.Json.Serialization.JsonPropertyName("supportDeltaPush")]
        public bool SupportDeltaPush { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("supportRemoteMetric")]
        public bool SupportRemoteMetric { get; set; }
    }
}
