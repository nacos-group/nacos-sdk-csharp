namespace Nacos.AspNetCore
{
    using System.Collections.Generic;

    public interface ILBStrategy
    {
        /// <summary>
        /// Strategy Name
        /// </summary>
        LBStrategyName Name { get; }

        /// <summary>
        /// Get instance
        /// </summary>
        /// <param name="list">server list</param>
        /// <returns>The instance</returns>
        string GetInstance(List<NacosServer> list);
    }
}
