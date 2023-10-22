namespace Nacos.Tests.Config.Utils
{
    using Nacos.Config.Utils;
    using Nacos.Exceptions;
    using System.Collections.Generic;
    using Xunit;

    // TODO：等待代码合并后完善测试
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

        [Fact]
        public void Check_Key_Param_Should_Exception()
        {
            var dataIds = new List<string> { "dataId", "data Id" };
            var group = "*group";
            var datumId = "datumId/";

            var ex = Assert.Throws<NacosException>(() => ParamUtils.CheckKeyParam(new List<string>(), group));
            Assert.Equal("dataId invalid", ex.Message);

            ex = Assert.Throws<NacosException>(() => ParamUtils.CheckKeyParam(dataIds, group));
            Assert.Equal("dataId invalid", ex.Message);

            ex = Assert.Throws<NacosException>(() => ParamUtils.CheckKeyParam(dataIds[1], group));
            Assert.Equal("dataId invalid", ex.Message);

            ex = Assert.Throws<NacosException>(() => ParamUtils.CheckKeyParam(dataIds[0], group));
            Assert.Equal("group invalid", ex.Message);

            group = "group";
            ex = Assert.Throws<NacosException>(() => ParamUtils.CheckKeyParam(dataIds[0], group, datumId));
            Assert.Equal("datumId invalid", ex.Message);
        }

        [Fact]
        public void Check_Key_Param_By_DatumId_Should_Exception()
        {
            var datumId = "datumId/";
            var ex = Assert.Throws<NacosException>(() => ParamUtils.CheckKeyParam("dataId", "group", datumId));
            Assert.Equal("datumId invalid", ex.Message);
        }
    }
}
