namespace Nacos.Config.FilterImpl
{
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.Options;
    using Nacos;
    using Nacos.Config.Abst;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ConfigFilterChainManager : IConfigFilterChain
    {
        private readonly List<IConfigFilter> filters = new();

        private readonly NacosSdkOptions _options;

        public ConfigFilterChainManager(NacosSdkOptions options)
        {
            _options = options;
            InitConfigFilters();
        }

        public ConfigFilterChainManager(IOptions<NacosSdkOptions> optionAccs)
        {
            _options = optionAccs.Value;
            InitConfigFilters();
        }

        private void InitConfigFilters()
        {
            List<IConfigFilter> configFilters =
                 GetAssemblies(_options).SelectMany(item => item.GetTypes())
                          .Where(item => item.GetInterfaces().Contains(typeof(IConfigFilter)))
                          .Select(type => (IConfigFilter)System.Activator.CreateInstance(type)).ToList();

            foreach (var configFilter in configFilters)
            {
                configFilter.Init(_options);
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
