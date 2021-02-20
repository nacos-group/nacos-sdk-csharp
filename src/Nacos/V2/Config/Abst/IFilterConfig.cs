namespace Nacos.V2.Config.Abst
{
    public interface IFilterConfig
    {
        string GetFilterName();

        object GetInitParameter(string name);
    }
}
