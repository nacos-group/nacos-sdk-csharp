namespace Nacos.Remote
{
    public interface IRequester
    {
        System.Threading.Tasks.Task<CommonResponse> RequestAsync(CommonRequest req, CommonRequestMeta meta);

        System.Threading.Tasks.Task<CommonResponse> RequestAsync(CommonRequest req, CommonRequestMeta meta, long timeoutMills);

        System.Collections.Generic.Dictionary<string, string> GetLabels();

        System.Threading.Tasks.Task CloseAsync();
    }
}
