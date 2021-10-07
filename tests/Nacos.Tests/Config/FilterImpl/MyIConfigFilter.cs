namespace Nacos.Tests.Config.FilterImpl
{
    using Nacos.V2;
    using Nacos.V2.Config.Abst;
    using System.Collections.Generic;

    public class MyIConfigFilter : IConfigFilter
    {
        private readonly string _name;
        private readonly int _order;

        public MyIConfigFilter(string name, int order)
        {
            this._name = name;
            this._order = order;
        }

        public void DoFilter(IConfigRequest request, IConfigResponse response, IConfigFilterChain filterChain)
        {
            IConfigContext configContext = request.GetConfigContext();
            configContext.SetParameter(_name, _order);

            if (configContext.GetParameter("orders") == null)
            {
                configContext.SetParameter("orders", new List<int>());
            }

            List<int> orders = (List<int>)configContext.GetParameter("orders");
            orders.Add(_order);

            if (configContext.GetParameter("filterCount") == null)
            {
                configContext.SetParameter("filterCount", 0);
            }

            int filterCount = (int)configContext.GetParameter("filterCount");
            filterCount = filterCount + 1;
            configContext.SetParameter("filterCount", filterCount);

            configContext.SetParameter("dofilter", _name);

            filterChain.DoFilter(request, response);
        }

        public string GetFilterName() => _name;

        public int GetOrder() => _order;

        public void Init(NacosSdkOptions options)
        {
        }
    }
}
