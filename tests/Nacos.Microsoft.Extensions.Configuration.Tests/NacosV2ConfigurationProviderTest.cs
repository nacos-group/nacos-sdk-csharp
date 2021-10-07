namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using Moq;
    using Nacos.V2;
    using Nacos.V2.Utils;
    using Xunit;
    using static Nacos.Microsoft.Extensions.Configuration.NacosV2ConfigurationProvider;

    [Trait("Category", "all")]
    public class NacosV2ConfigurationProviderTest
    {
        private Mock<INacosConfigService> _mockSvc;

        public NacosV2ConfigurationProviderTest()
        {
            _mockSvc = new Mock<INacosConfigService>();
        }

        [Fact]
        public void Init_Should_ThrowException_When_Listeners_Is_Empty()
        {
            Assert.Throws<Nacos.V2.Exceptions.NacosException>(() =>
            {
                new NacosV2ConfigurationProvider(new NacosV2ConfigurationSource());
            });
        }

        [Fact]
        public void Load_Should_Keep_The_Last_One_When_Contains_Same_Key()
        {
            var provider = GetProviderForMultiListeners();
            provider.Load();

            provider.TryGet("all", out var all);

            Assert.Equal("d2", all);
        }

        [Fact]
        public void Get_Provider_Data_IgnoreCase_Should_Succeed()
        {
            var provider = GetProviderForSingleListeners();
            provider.Load();

            Assert.True(provider.TryGet("all", out var all1));
            Assert.True(provider.TryGet("All", out var all2));
            Assert.Equal(all1, all2);
        }

        [Fact]
        public void MsConfigListener_Should_Not_Overwrite_When_Contains_Same_Key_And_Receive_First_One()
        {
            var provider = GetProviderForMultiListeners();
            provider.Load();

            MsConfigListener l1 = new MsConfigListener("d1", "g", false, provider, null);
            MsConfigListener l2 = new MsConfigListener("d2", "g", false, provider, null);

            provider.SetListener("d1#g", l1);
            provider.SetListener("d2#g", l2);

            l1.ReceiveConfigInfo(new { all = "d1_1" }.ToJsonString());

            provider.TryGet("all", out var all);

            Assert.Equal("d2", all);
        }

        [Fact]
        public void MsConfigListener_Should_Not_Overwrite_When_Contains_Same_Key_And_Receive_Second_One()
        {
            var provider = GetProviderForMultiListeners();
            provider.Load();

            MsConfigListener l1 = new MsConfigListener("d1", "g", false, provider, null);
            MsConfigListener l2 = new MsConfigListener("d2", "g", false, provider, null);

            provider.SetListener("d1#g", l1);
            provider.SetListener("d2#g", l2);

            l2.ReceiveConfigInfo(new { all = "d2_1" }.ToJsonString());

            provider.TryGet("all", out var all);

            Assert.Equal("d2_1", all);
        }

        private NacosV2ConfigurationProvider GetProviderForMultiListeners()
        {
            _mockSvc.Setup(x => x.GetConfig("d1", "g", 3000)).ReturnsAsync(new { all = "d1" }.ToJsonString());
            _mockSvc.Setup(x => x.GetConfig("d2", "g", 3000)).ReturnsAsync(new { all = "d2" }.ToJsonString());

            var cs = new NacosV2ConfigurationSource()
            {
                Namespace = "cs",
                Listeners = new System.Collections.Generic.List<ConfigListener>
                {
                     new ConfigListener { DataId = "d1", Group = "g" },
                     new ConfigListener { DataId = "d2", Group = "g" }
                }
            };

            var provider = new NacosV2ConfigurationProvider(cs, null, _mockSvc.Object, DefaultJsonConfigurationStringParser.Instance);
            return provider;
        }

        private NacosV2ConfigurationProvider GetProviderForSingleListeners()
        {
            _mockSvc.Setup(x => x.GetConfig("d1", "g", 3000)).ReturnsAsync(new { all = "d1" }.ToJsonString());

            var cs = new NacosV2ConfigurationSource()
            {
                Namespace = "cs",
                Listeners = new System.Collections.Generic.List<ConfigListener>
                {
                     new ConfigListener { DataId = "d1", Group = "g" }
                }
            };

            var provider = new NacosV2ConfigurationProvider(cs, null, _mockSvc.Object, DefaultJsonConfigurationStringParser.Instance);
            return provider;
        }
    }
}
