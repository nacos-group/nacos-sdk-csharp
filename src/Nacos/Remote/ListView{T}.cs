namespace Nacos.Remote
{
    using System.Collections.Generic;

    public class ListView<T>
    {
        public ListView(int count, List<T> data)
        {
            Count = count;
            Data = data;
        }

        [System.Text.Json.Serialization.JsonPropertyName("count")]
        public int Count { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public List<T> Data { get; set; }

        public override string ToString() => "ListView{" + "data=" + Data + ", count=" + Count + '}';
    }
}
