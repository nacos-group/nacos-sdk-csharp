namespace Nacos.Config.Abst
{
    public interface IConfigFilterChain
    {
        void AddFilter(IConfigFilter filter);

        /// <summary>
        /// Filter action.
        /// </summary>
        /// <param name="request">request</param>
        /// <param name="response">response</param>
        void DoFilter(IConfigRequest request, IConfigResponse response);
    }
}
