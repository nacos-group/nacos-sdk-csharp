namespace Nacos.Tests.Auth
{
    using Moq;
    using Nacos.Auth;
    using Nacos.Auth.Ram;
    using Nacos.Auth.Ram.Injector;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;

    [Trait("Category", "all")]
    public class RamClientAuthServiceImplTest
    {
        private readonly RamClientAuthServiceImpl _auth;
        private Mock<AbstractResourceInjector> mockResourceInjector;

        public RamClientAuthServiceImplTest()
        {
            mockResourceInjector = new Mock<AbstractResourceInjector>();
            var dict = new Dictionary<string, AbstractResourceInjector>()
            {
                { "mock", mockResourceInjector.Object }
            };

            _auth = new RamClientAuthServiceImpl(dict);
        }

        [Fact]
        public async Task Login_With_AKSK_Should_Succeed()
        {
            var flag = await _auth.Login(new NacosSdkOptions { AccessKey = "ak", SecretKey = "sk" }).ConfigureAwait(false);
            Assert.True(flag);

            Assert.Equal("ak", _auth.GetRamContext().AccessKey);
            Assert.Equal("sk", _auth.GetRamContext().SecretKey);
            Assert.Null(_auth.GetRamContext().RamRoleName);

            flag = await _auth.Login(new NacosSdkOptions { RamRoleName = "role" }).ConfigureAwait(false);
            Assert.True(flag);

            Assert.Equal("ak", _auth.GetRamContext().AccessKey);
            Assert.Equal("sk", _auth.GetRamContext().SecretKey);
            Assert.Null(_auth.GetRamContext().RamRoleName);
        }

        [Fact]
        public async Task Login_With_RoleName_Should_Succeed()
        {
            var flag = await _auth.Login(new NacosSdkOptions { RamRoleName = "role" }).ConfigureAwait(false);
            Assert.True(flag);

            Assert.Empty(_auth.GetRamContext().AccessKey);
            Assert.Empty(_auth.GetRamContext().SecretKey);
            Assert.Equal("role", _auth.GetRamContext().RamRoleName);

            flag = await _auth.Login(new NacosSdkOptions { AccessKey = "ak", SecretKey = "sk" }).ConfigureAwait(false);
            Assert.True(flag);

            Assert.Empty(_auth.GetRamContext().AccessKey);
            Assert.Empty(_auth.GetRamContext().SecretKey);
            Assert.Equal("role", _auth.GetRamContext().RamRoleName);
        }

        [Fact]
        public void GetLoginIdentityContext_Without_Login()
        {
            var ctx = _auth.GetLoginIdentityContext(new RequestResource("config", "", "", ""));
            Assert.Empty(ctx.GetAllKey());
            mockResourceInjector.Verify(x => x.DoInject(It.IsAny<RequestResource>(), It.IsAny<RamContext>(), It.IsAny<LoginIdentityContext>()), Times.Never);
        }

        [Fact]
        public async Task GetLoginIdentityContext_Without_Injector()
        {
            var resource = new RequestResource("config", "", "", "");
            await _auth.Login(new NacosSdkOptions { AccessKey = "ak", SecretKey = "sk" }).ConfigureAwait(false);
            var ctx = _auth.GetLoginIdentityContext(resource);
            Assert.Empty(ctx.GetAllKey());
            mockResourceInjector.Verify(x => x.DoInject(It.IsAny<RequestResource>(), It.IsAny<RamContext>(), It.IsAny<LoginIdentityContext>()), Times.Never);
        }

        [Fact]
        public async Task GetLoginIdentityContext_With_Injector()
        {
            var resource = new RequestResource("mock", "", "", "");
            await _auth.Login(new NacosSdkOptions { AccessKey = "ak", SecretKey = "sk" }).ConfigureAwait(false);
            var ctx = _auth.GetLoginIdentityContext(resource);
            Assert.Empty(ctx.GetAllKey());
            mockResourceInjector.Verify(x => x.DoInject(resource, It.IsAny<RamContext>(), It.IsAny<LoginIdentityContext>()), Times.Once);
        }
    }
}
