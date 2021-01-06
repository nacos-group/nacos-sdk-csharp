namespace Nacos.V2.Config.FilterImpl
{
    using System.Collections.Generic;

    public class ConfigFilterChainManager : IConfigFilterChain
    {
        private List<IConfigFilter> filters = new List<IConfigFilter>();

        public void DoFilter(IConfigRequest request, IConfigResponse response)
        {
            new VirtualFilterChain(this.filters).DoFilter(request, response);
        }

        public ConfigFilterChainManager AddFilter(IConfigFilter filter)
        {
            // 根据order大小顺序插入
            int i = 0;
            while (i < this.filters.Count)
            {
                IConfigFilter currentValue = this.filters[i];
                if (currentValue.GetFilterName().Equals(filter.GetFilterName()))
                {
                    break;
                }

                if (filter.GetOrder() >= currentValue.GetOrder() && i < this.filters.Count)
                {
                    i++;
                }
                else
                {
                    this.filters[i] = filter;
                    break;
                }
            }

            if (i == this.filters.Count)
            {
                this.filters[i] = filter;
            }

            return this;
        }

        internal class VirtualFilterChain : IConfigFilterChain
        {
            private readonly List<IConfigFilter> additionalFilters;

            private int currentPosition = 0;

            public VirtualFilterChain(List<IConfigFilter> additionalFilters)
            {
                this.additionalFilters = additionalFilters;
            }

            public void DoFilter(IConfigRequest request, IConfigResponse response)
            {
                if (this.currentPosition != this.additionalFilters.Count)
                {
                    this.currentPosition++;
                    IConfigFilter nextFilter = this.additionalFilters[this.currentPosition - 1];
                    nextFilter.DoFilter(request, response, this);
                }
            }
        }
    }
}
