namespace Nacos.Microsoft.Extensions.Configuration.Tests
{
    using Moq;
    using Nacos.V2;
    using Nacos.V2.Utils;
    using System.Threading.Tasks;
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
            var cs = new NacosV2ConfigurationSource(null, null)
            {
                Namespace = "cs",
                Listeners = new System.Collections.Generic.List<ConfigListener>
                {
                }
            };

            Assert.Throws<Nacos.V2.Exceptions.NacosException>(() =>
            {
                new NacosV2ConfigurationProvider(cs, null, null);
            });
        }

        [Fact]
        public void Init_Should_Call_AddListener()
        {
            _mockSvc.Setup(x => x.AddListener("d1", "g", It.IsAny<IListener>())).Returns(Task.CompletedTask);
            _mockSvc.Setup(x => x.AddListener("d2", "g", "ns", It.IsAny<IListener>())).Returns(Task.CompletedTask);

            var cs = new NacosV2ConfigurationSource(null, null)
            {
                Namespace = "cs",
                Listeners = new System.Collections.Generic.List<ConfigListener>
                {
                    new ConfigListener { DataId = "d1", Group = "g" },
                    new ConfigListener { DataId = "d2", Group = "g", Namespace = "ns" }
                },
                NacosConfigurationParser = DefaultJsonConfigurationStringParser.Instance
            };

            var provider = new NacosV2ConfigurationProvider(cs, _mockSvc.Object, null);

            _mockSvc.Verify(x => x.AddListener("d1", "g", It.IsAny<IListener>()), Times.Once);
            _mockSvc.Verify(x => x.AddListener("d2", "g", "ns", It.IsAny<IListener>()), Times.Once);
        }

        [Fact]
        public void Dispose_Should_Call_RemoveListener()
        {
            var provider = GetProviderForMultiListeners();
            _mockSvc.Setup(x => x.RemoveListener("d1", "g", It.IsAny<IListener>())).Returns(Task.CompletedTask);
            _mockSvc.Setup(x => x.RemoveListener("d2", "g", "ns", It.IsAny<IListener>())).Returns(Task.CompletedTask);
            provider.Dispose();
            _mockSvc.Verify(x => x.RemoveListener("d1", "g", It.IsAny<IListener>()), Times.Once);
            _mockSvc.Verify(x => x.RemoveListener("d2", "g", "ns", It.IsAny<IListener>()), Times.Once);
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
        public void Load_Should_Fail_When_IsNotOptional()
        {
            var provider = GetProviderForSingleListeners(true, false);
            provider.Load();

            Assert.Empty(provider.GetData());
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

            MsConfigListener l1 = new MsConfigListener(new ConfigListener() { DataId = "d1", Group = "g" }, provider, null);
            MsConfigListener l2 = new MsConfigListener(new ConfigListener() { DataId = "d2", Group = "g", Namespace = "ns" }, provider, null);

            provider.SetListener("cs#d1#g", l1);
            provider.SetListener("ns#d2#g", l2);

            l1.ReceiveConfigInfo(new { all = "d1_1" }.ToJsonString());

            provider.TryGet("all", out var all);

            Assert.Equal("d2", all);
        }

        [Fact]
        public void MsConfigListener_Should_Not_Overwrite_When_Contains_Same_Key_And_Receive_Second_One()
        {
            var provider = GetProviderForMultiListeners();
            provider.Load();

            MsConfigListener l1 = new MsConfigListener(new ConfigListener() { DataId = "d1", Group = "g" }, provider, null);
            MsConfigListener l2 = new MsConfigListener(new ConfigListener() { DataId = "d2", Group = "g", Namespace = "ns" }, provider, null);

            provider.SetListener("cs#d1#g", l1);
            provider.SetListener("ns#d2#g", l2);

            l2.ReceiveConfigInfo(new { all = "d2_1" }.ToJsonString());

            provider.TryGet("all", out var all);

            Assert.Equal("d2_1", all);
        }

        private NacosV2ConfigurationProvider GetProviderForMultiListeners()
        {
            _mockSvc.Setup(x => x.GetConfig("d1", "g", 3000)).ReturnsAsync(new { all = "d1" }.ToJsonString());
            _mockSvc.Setup(x => x.GetConfig("d2", "g", "ns", 3000)).ReturnsAsync(new { all = "d2" }.ToJsonString());

            var cs = new NacosV2ConfigurationSource(null, null)
            {
                Namespace = "cs",
                Listeners = new System.Collections.Generic.List<ConfigListener>
                {
                     new ConfigListener { DataId = "d1", Group = "g" },
                     new ConfigListener { DataId = "d2", Group = "g", Namespace = "ns" }
                },
                NacosConfigurationParser = DefaultJsonConfigurationStringParser.Instance
            };

            var provider = new NacosV2ConfigurationProvider(cs, _mockSvc.Object, null);
            return provider;
        }

        private NacosV2ConfigurationProvider GetProviderForSingleListeners(bool isThrow = false, bool isOptional = true)
        {
            var setup = _mockSvc.Setup(x => x.GetConfig("d1", "g", 3000));

            if (!isThrow)
            {
                setup.ReturnsAsync(new { all = "d1" }.ToJsonString());
            }
            else
            {
                setup.ThrowsAsync(new System.Exception("ts"));
            }

            var cs = new NacosV2ConfigurationSource(null, null)
            {
                Namespace = "cs",
                Listeners = new System.Collections.Generic.List<ConfigListener>
                {
                     new ConfigListener { DataId = "d1", Group = "g", Optional = isOptional }
                },
                NacosConfigurationParser = DefaultJsonConfigurationStringParser.Instance
            };

            var provider = new NacosV2ConfigurationProvider(cs, _mockSvc.Object, null);
            return provider;
        }
    }
}
