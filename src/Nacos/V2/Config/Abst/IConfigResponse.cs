namespace Nacos.V2.Config.Abst
{
    public interface IConfigResponse
    {
        object GetParameter(string key);

        IConfigContext GetConfigContext();
    }
}
