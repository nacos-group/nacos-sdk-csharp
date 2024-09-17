namespace Nacos.V2.Remote.Requests
{
    public class ClientAbilities
    {
        [System.Text.Json.Serialization.JsonPropertyName("remoteAbility")]
        public ClientRemoteAbility RemoteAbility = new ClientRemoteAbility();

        [System.Text.Json.Serialization.JsonPropertyName("configAbility")]
        public ClientConfigAbility ConfigAbility = new ClientConfigAbility();

        [System.Text.Json.Serialization.JsonPropertyName("namingAbility")]
        public ClientNamingAbility NamingAbility = new ClientNamingAbility();
    }
}
