namespace Nacos.V2.Config.FilterImpl
{
    public interface IConfigResponse
    {
        object GetParameter(string key);

        IConfigContext GetConfigContext();
    }
}
