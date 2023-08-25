namespace Nacos.Config.FilterImpl
{
    using Microsoft.Extensions.Options;
    using Nacos;
    using Nacos.Config.Abst;
    using System.Collections.Generic;

    public class ConfigFilterChainManager : IConfigFilterChain
    {
        private readonly List<IConfigFilter> filters = new();

        private readonly NacosSdkOptions _options;

        public ConfigFilterChainManager(IOptions<NacosSdkOptions> optionsAccs, IEnumerable<IConfigFilter> configFilters)
        {
            _options = optionsAccs.Value;

            foreach (var configFilter in configFilters)
            {
                AddFilter(configFilter);
            }
        }

        public void DoFilter(IConfigRequest request, IConfigResponse response)
        {
            new VirtualFilterChain(filters).DoFilter(request, response);
        }

        public void AddFilter(IConfigFilter filter)
        {
            filter.Init(_options);

            // ordered by order value
            int i = 0;
            while (i < filters.Count)
            {
                IConfigFilter currentValue = filters[i];
                if (currentValue.GetFilterName().Equals(filter.GetFilterName()))
                {
                    break;
                }

                if (filter.GetOrder() >= currentValue.GetOrder() && i < filters.Count)
                {
                    i++;
                }
                else
                {
                    filters.Insert(i, filter);
                    break;
                }
            }

            if (i == filters.Count)
            {
                filters.Insert(i, filter);
            }
        }

        internal class VirtualFilterChain : IConfigFilterChain
        {
            private readonly List<IConfigFilter> additionalFilters;

            private int currentPosition = 0;

            public VirtualFilterChain(List<IConfigFilter> additionalFilters)
            {
                this.additionalFilters = additionalFilters;
            }

            public void AddFilter(IConfigFilter filter)
            {
            }

            public void DoFilter(IConfigRequest request, IConfigResponse response)
            {
                if (currentPosition != additionalFilters.Count)
                {
                    currentPosition++;
                    IConfigFilter nextFilter = additionalFilters[currentPosition - 1];
                    nextFilter.DoFilter(request, response, this);
                }
            }
        }
    }
}
