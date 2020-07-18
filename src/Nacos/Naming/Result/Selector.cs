namespace Nacos
{
    public class Selector
    {
        /// <summary>
        /// The types of selector accepted by Nacos
        ///
        /// 1. unknown  not match any type
        /// 2. none     not filter out any entity
        /// 3. label    select by label
        ///
        /// </summary>
        public string Type { get; set; }
    }
}
