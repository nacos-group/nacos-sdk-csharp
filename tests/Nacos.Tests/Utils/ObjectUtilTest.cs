namespace Nacos.Tests.Utils
{
    using Nacos.Utils;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ObjectUtilTest
    {
        [Fact]
        public void ToObj_Should_Succeed()
        {
            var json = """
             {
                "num":1,
                "str":"str1",
                "child":
                {
                    "num":2,
                    "str":"str2"
                },
                "childs":
                [
                    {
                        "num":3,
                        "str":"str3"
                    },
                    {
                        "num":4,
                        "str":"str4"
                    }
                ]
             }
             """;
            var obj = json.ToObj<ObjectUtilTestModel>();

            Assert.Equal(1, obj.Num);
            Assert.Equal("str1", obj.Str);

            Assert.Equal(2, obj.Child.Num);
            Assert.Equal("str2", obj.Child.Str);

            Assert.Equal(4, obj.Childs.LastOrDefault().Num);
            Assert.Equal("str4", obj.Childs.LastOrDefault().Str);
        }

        [Fact]
        public void SpecialChar_ToJsonString_And_ToObj_Should_Succeed()
        {
            var obj = new ObjectUtilTestModel
            {
                Str = "， 、 。 ． ？ ！ ～ ＄ ％ ＠ ＆ ＃ ＊ ? ； ︰ … ‥ ﹐ ﹒ ˙ ? ‘ ’ “ ” 〝 〞 ‵ ′ 〃 ↑ ↓ ← → ↖ ↗ ↙ ↘ ㊣ ◎ ○ ● ⊕ ⊙ ○ ● △ ▲ ☆ ★ ◇ ◆ □ ■ ▽ ▼ § ￥ 〒 ￠ ￡ ※ ♀ ♂"
            };
            var json = obj.ToJsonString();
            var obj1 = json.ToObj<ObjectUtilTestModel>();
            Assert.Equal(obj.Str, obj1.Str);
        }

        public class ObjectUtilTestModel
        {
            public int Num { get; set; }

            public string Str { get; set; }

            public ObjectUtilTestModel Child { get; set; }

            public List<ObjectUtilTestModel> Childs { get; set; }
        }
    }
}
