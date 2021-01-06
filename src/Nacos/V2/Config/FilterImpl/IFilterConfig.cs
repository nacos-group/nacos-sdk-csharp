namespace Nacos.V2.Config.FilterImpl
{
    public interface IFilterConfig
    {
        string GetFilterName();

        object GetInitParameter(string name);
    }
}
