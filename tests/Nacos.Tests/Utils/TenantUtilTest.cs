namespace Nacos.Tests.Utils
{
    using Google.Protobuf.WellKnownTypes;
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class TenantUtilTest
    {
        [Fact]
        public void GetUserTenantForAcm_Should_Succeed()
        {
            var tenant = TenantUtil.GetUserTenantForAcm();
            Assert.Equal(string.Empty, tenant);

            Environment.SetEnvironmentVariable("tenant.id", "acm.tenant.id.value");
            tenant = TenantUtil.GetUserTenantForAcm();
            Assert.Equal("acm.tenant.id.value", tenant);

            Environment.SetEnvironmentVariable("tenant.id", string.Empty);
            Environment.SetEnvironmentVariable("acm.namespace", "acm.namespace.value");
            tenant = TenantUtil.GetUserTenantForAcm();
            Assert.Equal("acm.namespace.value", tenant);
        }

        [Fact]
        public void GetUserTenantForAns_Should_Succeed()
        {
            var tenant = TenantUtil.GetUserTenantForAns();
            Assert.Equal(string.Empty, tenant);

            Environment.SetEnvironmentVariable("tenant.id", "ans.tenant.id.value");
            tenant = TenantUtil.GetUserTenantForAns();
            Assert.Equal("ans.tenant.id.value", tenant);

            Environment.SetEnvironmentVariable("tenant.id", string.Empty);
            Environment.SetEnvironmentVariable("ans.namespace", "ans.namespace.value");
            tenant = TenantUtil.GetUserTenantForAns();
            Assert.Equal("ans.namespace.value", tenant);
        }
    }
}
