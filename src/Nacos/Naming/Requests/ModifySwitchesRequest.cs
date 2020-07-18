namespace Nacos
{
    using System.Collections.Generic;
    using System.Text;

    public class ModifySwitchesRequest : BaseRequest
    {
        /// <summary>
        /// switch name
        /// </summary>
        public string Entry { get; set; }

        /// <summary>
        /// switch value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// if affect the local server, true means yes, false means no, default true
        /// </summary>
        public bool? Debug { get; set; }

        public override void CheckParam()
        {
        }

        public override Dictionary<string, string> ToDict()
        {
            var dict = new Dictionary<string, string>
            {
                { "entry", Entry },
                { "value", Value },
            };

            if (Debug.HasValue)
                dict.Add("debug", Debug.Value.ToString());

            return dict;
        }
    }
}
