namespace Nacos
{
    using System.Collections.Generic;

    public class ListClusterServersRequest : BaseRequest
    {
        /// <summary>
        /// if return healthy servers only
        /// </summary>
        public bool? Healthy { get; set; }

        public override void CheckParam()
        {
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>();

            if (Healthy.HasValue)
                dict.Add("healthy", Healthy.Value.ToString());

            return dict;
        }
    }
}
