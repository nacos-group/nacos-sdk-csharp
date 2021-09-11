namespace Nacos.AspNetCore.Tests
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
    }
}
