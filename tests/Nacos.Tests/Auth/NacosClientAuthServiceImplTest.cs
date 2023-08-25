namespace Nacos.Tests.Auth
{
    using Moq.Protected;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.Extensions.Logging.Abstractions;
    using Nacos.Auth;

    [Trait("Category", "all")]
    public class NacosClientAuthServiceImplTest
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

            var serverList = new List<string> { "localhost" };

            Nacos.Auth.NacosClientAuthServiceImpl auth = new(NullLoggerFactory.Instance, new HttpClient(mockHttpMessageHandler.Object));
            auth.SetServerList(serverList);

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var ret = await auth.Login(options).ConfigureAwait(false);

            Assert.True(ret);
        }

        [Fact]
        public async Task Login_FailCode()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(METHOD, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });

            var serverList = new List<string> { "localhost" };

            Nacos.Auth.NacosClientAuthServiceImpl auth = new(NullLoggerFactory.Instance, new HttpClient(mockHttpMessageHandler.Object));
            auth.SetServerList(serverList);

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var ret = await auth.Login(options).ConfigureAwait(false);

            Assert.False(ret);
        }

        [Fact]
        public async Task Login_FailHttp()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(METHOD, ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception());

            var serverList = new List<string> { "localhost" };

            Nacos.Auth.NacosClientAuthServiceImpl auth = new(NullLoggerFactory.Instance, new HttpClient(mockHttpMessageHandler.Object));
            auth.SetServerList(serverList);

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var ret = await auth.Login(options).ConfigureAwait(false);

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

            var serverList = new List<string> { "localhost" };

            Nacos.Auth.NacosClientAuthServiceImpl auth = new(NullLoggerFactory.Instance, new HttpClient(mockHttpMessageHandler.Object));
            auth.SetServerList(serverList);

            var options = new NacosSdkOptions() { UserName = "aaa", Password = "123456" };
            var ret = await auth.Login(options).ConfigureAwait(false);
            Assert.True(ret);

            var acc = auth.GetLoginIdentityContext(null).GetParameter(NacosAuthLoginConstant.ACCESSTOKEN);
            Assert.Equal("abc", acc);
        }
    }
}
