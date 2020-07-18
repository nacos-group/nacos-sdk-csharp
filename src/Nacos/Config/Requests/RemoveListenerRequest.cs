namespace Nacos
{
    using System;
    using System.Collections.Generic;
    using Nacos.Utilities;

    public class RemoveListenerRequest : BaseRequest
    {
        /// <summary>
        /// Tenant information. It corresponds to the Namespace field in Nacos.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Configuration ID
        /// </summary>
        public string DataId { get; set; }

        /// <summary>
        /// Configuration group
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Callbacks when configuration was changed
        /// </summary>
        /// <value>The callbacks.</value>
        public List<Action> Callbacks { get; set; } = new List<Action>();

        public override void CheckParam()
        {
            ParamUtil.CheckTDG(Tenant, DataId, Group);
        }

        public override Dictionary<string, string> ToDict()
        {
            return new Dictionary<string, string>();
        }
    }
}
