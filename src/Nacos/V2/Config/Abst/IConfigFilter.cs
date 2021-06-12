namespace Nacos.V2.Config.Abst
{
    public interface IConfigFilter
    {
        void Init(NacosSdkOptions options);

        void DoFilter(IConfigRequest request, IConfigResponse response, IConfigFilterChain filterChain);

        int GetOrder();

        string GetFilterName();
    }
}
