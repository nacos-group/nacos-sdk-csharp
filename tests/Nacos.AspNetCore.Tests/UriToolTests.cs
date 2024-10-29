﻿namespace Nacos.AspNetCore.Tests
{
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using System.Linq;
    using Xunit;

    [Trait("Category", "all")]
    public class UriToolTests
    {
        [Fact]
        public void GetUri_Should_Return_Single_Uri()
        {
            IFeatureCollection fc = new FeatureCollection();
            IServerAddressesFeature saf = new ServerAddressesFeature();
            saf.Addresses.Add("http://*:8080");
            fc.Set<IServerAddressesFeature>(saf);

            var uris = UriTool.GetUri(fc, "", 0, "");

            Assert.Single(uris);
        }

        [Fact]
        public void GetUri_Should_Return_Multi_Uris()
        {
            IFeatureCollection fc = new FeatureCollection();
            IServerAddressesFeature saf = new ServerAddressesFeature();
            saf.Addresses.Add("http://*:8080");
            saf.Addresses.Add("http://*:8081");
            fc.Set<IServerAddressesFeature>(saf);

            var uris = UriTool.GetUri(fc, "", 0, "");

            Assert.Equal(2, uris.Count());
        }

        [Theory]
        [InlineData("http://+80")]
        [InlineData("http://*80")]
        public void GetUrl_With_ASPNETCORE_URLS_Should_ThrowExceptions(string url)
        {
            System.Environment.SetEnvironmentVariable("ASPNETCORE_URLS", url);
            var ex = Assert.Throws<Nacos.V2.Exceptions.NacosException>(() => UriTool.GetUri(null, "", 0, ""));
            Assert.Equal("Invalid ip address from ASPNETCORE_URLS", ex.ErrorMsg);
        }

        [Theory]
        [InlineData("http://+:80")]
        [InlineData("http://*:80")]
        public void GetUrl_With_ASPNETCORE_URLS_Should_Succeed(string url)
        {
            System.Environment.SetEnvironmentVariable("ASPNETCORE_URLS", url);
            var uris = UriTool.GetUri(null, "", 0, "");
            Assert.Single(uris);
            var uri = uris.First();
            Assert.True(System.Net.IPAddress.TryParse(uri.Host, out _));
            Assert.Equal(80, uri.Port);
        }

        [Fact]
        public void GetUri_With_Multiple_And_Regex_PreferredNetworks_Should_Succeed()
        {
            var preferredNetworks = @"192.168.,10.0.,172\.\d+\.\d+\.\d+";
            var uris = UriTool.GetUri(null, "", 0, preferredNetworks);
            Assert.Single(uris);
            var uri = uris.First();
            Assert.Matches(@"(192\.168\.\d+\.\d+|10\.0\.\d+\.\d+|172\.\d+\.\d+|127.0.0.1)", uri.Host);
        }
    }
}
