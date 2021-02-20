namespace Nacos.V2.Remote
{
    using System.Collections.Generic;

    public class ListView<T>
    {
        public ListView(int count, List<T> data)
        {
            this.Count = count;
            this.Data = data;
        }

        [Newtonsoft.Json.JsonProperty("count")]
        public int Count { get; set; }

        [Newtonsoft.Json.JsonProperty("data")]
        public List<T> Data { get; set; }

        public override string ToString() => "ListView{" + "data=" + Data + ", count=" + Count + '}';
    }
}
