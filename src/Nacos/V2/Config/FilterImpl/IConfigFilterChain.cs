namespace Nacos.V2.Config.FilterImpl
{
    public interface IConfigFilterChain
    {
        /// <summary>
        /// Filter action.
        /// </summary>
        /// <param name="request">request</param>
        /// <param name="response">response</param>
        void DoFilter(IConfigRequest request, IConfigResponse response);
    }
}
