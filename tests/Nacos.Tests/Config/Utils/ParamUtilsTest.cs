namespace Nacos.Tests.Config.Utils
{
    using Nacos.Config.Utils;
    using Nacos.Exceptions;
    using Xunit;

    public class ParamUtilsTest
    {
        [Fact]
        public void IsValid_Should_Succeed()
        {
            string param = null;
            var isValid = ParamUtils.IsValid(param);
            Assert.False(isValid);

            param = "c_c/cc@ccc";
            isValid = ParamUtils.IsValid(param);
            Assert.False(isValid);

            param = "cc_cc-ccc.vvvv:";
            isValid = ParamUtils.IsValid(param);
            Assert.True(isValid);
        }

        [Fact]
        public void Check_Tenant_DataId_Group_Should_Exception()
        {
            var tenant = "@tenant";
            var dataId = "data Id";
            var group = "*group";

            var ex = Assert.Throws<NacosException>(() => ParamUtils.CheckTdg(tenant, dataId, group));
            Assert.Equal("tenant invalid", ex.Message);

            tenant = "tenant";
            ex = Assert.Throws<NacosException>(() => ParamUtils.CheckTdg(tenant, dataId, group));
            Assert.Equal("dataId invalid", ex.Message);

            dataId = "dataId";
            ex = Assert.Throws<NacosException>(() => ParamUtils.CheckTdg(tenant, dataId, group));
            Assert.Equal("group invalid", ex.Message);
        }
    }
}
