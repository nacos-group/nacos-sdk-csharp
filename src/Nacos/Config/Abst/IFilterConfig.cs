namespace Nacos.Config.Abst
{
    public interface IFilterConfig
    {
        string GetFilterName();

        object GetInitParameter(string name);
    }
}
