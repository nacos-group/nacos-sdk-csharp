namespace Nacos.Tests.V2.Filters
{
    using Nacos;
    using Nacos.Config.Abst;

    public class ConfigBaseTestFilter : IConfigFilter
    {
        public void DoFilter(IConfigRequest request, IConfigResponse response, IConfigFilterChain filterChain)
        {
            if (request != null)
            {
                var raw_content = request.GetParameter(Nacos.Config.Common.ConfigConstants.CONTENT);
                request.PutParameter(Nacos.Config.Common.ConfigConstants.CONTENT, raw_content + "-request");
            }

            if (response != null)
            {
                var resp_content = response.GetParameter(Nacos.Config.Common.ConfigConstants.CONTENT);

                response.PutParameter(Nacos.Config.Common.ConfigConstants.CONTENT, resp_content + "-response");
            }
        }

        public string GetFilterName() => nameof(ConfigBaseTestFilter);

        public int GetOrder() => 1;

        public void Init(NacosSdkOptions options)
        {
        }
    }
}
