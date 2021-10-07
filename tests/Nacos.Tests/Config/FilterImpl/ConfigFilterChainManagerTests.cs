namespace Nacos.Tests.Config.FilterImpl
{
    using Nacos.V2;
    using Nacos.V2.Config.Abst;
    using Nacos.V2.Config.FilterImpl;
    using System.Collections.Generic;
    using Xunit;

    [Trait("Category", "all")]
    public class ConfigFilterChainManagerTests
    {
        [Fact]
        public void AddFilter_Order_Should_Right()
        {
            ConfigFilterChainManager configFilterChainManager = new ConfigFilterChainManager(new NacosSdkOptions());
            var filter1 = new MyIConfigFilter("filter1", 1);
            var filter2 = new MyIConfigFilter("filter2", 2);
            var filter3 = new MyIConfigFilter("filter3", 3);

            // random order
            configFilterChainManager.AddFilter(filter2);
            configFilterChainManager.AddFilter(filter1);
            configFilterChainManager.AddFilter(filter3);

            ConfigRequest configRequest = new ConfigRequest();

            configFilterChainManager.DoFilter(configRequest, new ConfigResponse());

            IConfigContext configContext = configRequest.GetConfigContext();

            Assert.Equal(1, configContext.GetParameter("filter1"));
            Assert.Equal(2, configContext.GetParameter("filter2"));
            Assert.Equal(3, configContext.GetParameter("filter3"));

            var orders = (List<int>)configContext.GetParameter("orders");
            Assert.Equal(new List<int> { 1, 2, 3 }, orders);
        }

        [Fact]
        public void AddFilter_ShouldNot_Repeat_When_Filter_Is_Same()
        {
            ConfigFilterChainManager configFilterChainManager = new ConfigFilterChainManager(new NacosSdkOptions());
            var filter1 = new MyIConfigFilter("filter1", 1);
            var filter2 = new MyIConfigFilter("filter2", 2);
            var repeatFilter = new MyIConfigFilter("filter1", 1);

            configFilterChainManager.AddFilter(filter2);
            configFilterChainManager.AddFilter(filter1);
            configFilterChainManager.AddFilter(repeatFilter);

            ConfigRequest configRequest = new ConfigRequest();

            configFilterChainManager.DoFilter(configRequest, new ConfigResponse());

            IConfigContext configContext = configRequest.GetConfigContext();

            Assert.Equal(2, configContext.GetParameter("filterCount"));
        }

        [Fact]
        public void DoFilter_Should_With_Right_Order()
        {
            ConfigFilterChainManager configFilterChainManager = new ConfigFilterChainManager(new NacosSdkOptions());
            var filter1 = new MyIConfigFilter("filter1", 1);
            var filter2 = new MyIConfigFilter("filter2", 2);

            configFilterChainManager.AddFilter(filter2);
            configFilterChainManager.AddFilter(filter1);

            ConfigRequest configRequest = new ConfigRequest();

            configFilterChainManager.DoFilter(configRequest, new ConfigResponse());

            IConfigContext configContext = configRequest.GetConfigContext();

            Assert.Equal("filter2", configContext.GetParameter("dofilter"));
        }
    }
}
