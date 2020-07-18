namespace Nacos
{
    using System.Collections.Generic;

    public abstract class BaseRequest
    {
        /// <summary>
        /// Checks whether request is valid
        /// </summary>
        public abstract void CheckParam();

        /// <summary>
        /// Convert request to params of API
        /// </summary>
        /// <returns>Dictionary</returns>
        public abstract Dictionary<string, string> ToDict();
    }
}
