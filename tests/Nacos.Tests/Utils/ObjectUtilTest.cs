namespace Nacos.Tests.Utils
{
    using Nacos.Utils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class ObjectUtilTest
    {
        [Fact]
        public void ToTimestamp_Should_Succeed()
        {
            var dt = DateTime.Parse("2023-10-28 10:59:59");
            var timestamp = dt.ToTimestamp();
            Assert.Equal(1698490799, timestamp);
        }

        [Fact]
        public void SafeGetValue_Should_Succeed()
        {
            var dic = new Dictionary<string, object>();
            var value = dic.SafeGetValue("key", "default-value");
            Assert.Equal("default-value", value);

            dic.Add("key", "set-value");
            value = dic.SafeGetValue("key", "default-value");
            Assert.Equal("set-value", value);
        }

        [Fact]
        public async Task ReadFileAsync_Should_Succeed()
        {
            var writeContent = "written test content";
            var file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.txt"));
            await WriteAsync(writeContent, file.FullName).ConfigureAwait(false);

            var readContent = await file.ReadFileAsync().ConfigureAwait(false);
            Assert.Equal(writeContent, readContent);

            File.Delete(file.FullName);
        }

        private async Task WriteAsync(string content, string path)
        {
            try
            {
                using FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                fs.SetLength(bytes.Length);
                await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


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
