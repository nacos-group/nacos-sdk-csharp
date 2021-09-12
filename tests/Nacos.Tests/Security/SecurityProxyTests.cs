namespace Nacos.Tests.Security
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Moq.Protected;
    using Nacos.V2;
    using Nacos.V2.Security;
    using Xunit;

    [Trait("Category", "all")]
    public class SecurityProxyTests
    {
        private const string METHOD = "SendAsync";

        [Fact]
        public async Task Login_Should_Succeed()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(METHOD, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"accessToken\":\"ttttttttttttttttt\",\"tokenTtl\":1000}")
                });

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var securityProxy = new SecurityProxy(options, NullLogger.Instance, mockHttpMessageHandler.Object);

            bool ret = await securityProxy.LoginAsync("localhost").ConfigureAwait(false);

            Assert.True(ret);
        }

        [Fact]
        public async Task Login_Should_Fail()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(METHOD, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var securityProxy = new SecurityProxy(options, NullLogger.Instance, mockHttpMessageHandler.Object);

            bool ret = await securityProxy.LoginAsync("localhost").ConfigureAwait(false);

            Assert.False(ret);
        }

        [Fact]
        public async Task GetAccessToken_Should_Succeed()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(METHOD, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"accessToken\":\"abc\",\"tokenTtl\":1000}")
                });

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var securityProxy = new SecurityProxy(options, NullLogger.Instance, mockHttpMessageHandler.Object);

            await securityProxy.LoginAsync("localhost").ConfigureAwait(false);
            var accessToken = securityProxy.GetAccessToken();

            Assert.Equal("abc", accessToken);
        }

        [Fact]
        public async Task IsEnabled_Should_Succeed()
        {
            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var securityProxy = new SecurityProxy(options, NullLogger.Instance);

            await securityProxy.LoginAsync("localhost").ConfigureAwait(false);
            var isEnabled = securityProxy.IsEnabled();

            Assert.True(isEnabled);
        }

        [Fact]
        public async Task Login_Twice_In_Window_Should_Call_Once()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(METHOD, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"accessToken\":\"abc\",\"tokenTtl\":1000}")
                });

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var securityProxy = new SecurityProxy(options, NullLogger.Instance, mockHttpMessageHandler.Object);

            await securityProxy.LoginAsync(new List<string> { "localhost" }).ConfigureAwait(false);
            await securityProxy.LoginAsync(new List<string> { "localhost" }).ConfigureAwait(false);

            mockHttpMessageHandler.Protected().Verify(METHOD, Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task Login_Twice_NotIn_Window_Should_Call_Twice()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(METHOD, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"accessToken\":\"abc\",\"tokenTtl\":1}")
                });

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var securityProxy = new SecurityProxy(options, NullLogger.Instance, mockHttpMessageHandler.Object);

            await securityProxy.LoginAsync(new List<string> { "localhost" }).ConfigureAwait(false);

            await Task.Delay(1001).ConfigureAwait(false);

            await securityProxy.LoginAsync(new List<string> { "localhost" }).ConfigureAwait(false);

            mockHttpMessageHandler.Protected().Verify(METHOD, Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }
    }
}
