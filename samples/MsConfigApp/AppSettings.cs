namespace MsConfigApp
{
    using System.Collections.Generic;

    public class AppSettings
    {
        /*
     {
    "ConnectionStrings": {
        "Default": "Server=127.0.0.1;Port=3306;Database=demo;User Id=root;Password=123456;"
    },
    "version": "测试version",
    "AppSettings": {
        "Str": "val",
        "num": 1,
        "arr": [1, 2, 3],
        "subobj": {
            "a": "b"
        }
    }
}
         */

        public string Str { get; set; }

#pragma warning disable SA1300 // Element should begin with upper-case letter
        public int num { get; set; }

        public List<int> arr { get; set; }

        public SubObj subobj { get; set; }
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }
}
