namespace Nacos.V2.Config.FilterImpl
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyModel;
    using Nacos.V2.Config.Abst;

    public class ConfigFilterChainManager : IConfigFilterChain
    {
        private List<IConfigFilter> filters = new List<IConfigFilter>();

        public ConfigFilterChainManager(NacosSdkOptions options)
        {
            List<IConfigFilter> configFilters =
                GetAssemblies(options).SelectMany(item => item.GetTypes())
                                .Where(item => item.GetInterfaces().Contains(typeof(IConfigFilter)))
                                .Select(type => (IConfigFilter)System.Activator.CreateInstance(type)).ToList();

            foreach (var configFilter in configFilters)
            {
                configFilter.Init(options);
                AddFilter(configFilter);
            }
        }

        private List<Assembly> GetAssemblies(NacosSdkOptions options)
        {
            var assemblies = new List<Assembly>();

            if (options.ConfigFilterAssemblies == null || !options.ConfigFilterAssemblies.Any()) return assemblies;

            DependencyContext context = DependencyContext.Default;

            var libs = context.CompileLibraries.Where(lib => options.ConfigFilterAssemblies.Contains(lib.Name));

            foreach (var lib in libs)
            {
                var assembly = Assembly.Load(new AssemblyName(lib.Name));
                assemblies.Add(assembly);
            }

            return assemblies;
        }

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
                    this.filters.Insert(i, filter);
                    break;
                }
            }

            if (i == this.filters.Count)
            {
                this.filters.Insert(i, filter);
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
