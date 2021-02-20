namespace Nacos.V2.Remote.Requests
{
    public class ClientAbilities
    {
        [Newtonsoft.Json.JsonProperty("remoteAbility")]
        public ClientRemoteAbility RemoteAbility = new ClientRemoteAbility();

        [Newtonsoft.Json.JsonProperty("configAbility")]
        public ClientConfigAbility ConfigAbility = new ClientConfigAbility();

        [Newtonsoft.Json.JsonProperty("namingAbility")]
        public ClientNamingAbility NamingAbility = new ClientNamingAbility();
    }
}
