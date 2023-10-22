namespace Nacos.Config.Filter
{
    public interface IConfigFilter
    {
        void Init(NacosSdkOptions options);

        void DoFilter(IConfigRequest request, IConfigResponse response, IConfigFilterChain filterChain);

        int GetOrder();

        string GetFilterName();
    }
}
