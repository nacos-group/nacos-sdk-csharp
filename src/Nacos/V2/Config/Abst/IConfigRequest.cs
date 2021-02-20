namespace Nacos.V2.Config.Abst
{
    public interface IConfigRequest
    {
        object GetParameter(string key);

        IConfigContext GetConfigContext();
    }
}
