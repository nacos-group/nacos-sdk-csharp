namespace Nacos.V2.Config.Abst
{
    public interface IConfigContext
    {
        /// <summary>
        /// Get context param by key.
        /// </summary>
        /// <param name="key">parameter key</param>
        /// <returns>context</returns>
        object GetParameter(string key);

        /// <summary>
        /// Set context param.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        void SetParameter(string key, object value);
    }
}
