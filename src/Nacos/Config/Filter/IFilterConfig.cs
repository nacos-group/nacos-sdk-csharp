namespace Nacos.Config.Filter
{
    public interface IFilterConfig
    {
        string GetFilterName();

        object GetInitParameter(string name);
    }
}
