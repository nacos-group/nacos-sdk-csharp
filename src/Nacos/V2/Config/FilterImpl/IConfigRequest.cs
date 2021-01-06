namespace Nacos.V2.Config.FilterImpl
{
    public interface IConfigRequest
    {
        object GetParameter(string key);

        IConfigContext GetConfigContext();
    }
}
