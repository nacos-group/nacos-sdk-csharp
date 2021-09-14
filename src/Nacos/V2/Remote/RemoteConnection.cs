namespace Nacos.V2.Remote
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class RemoteConnection : IRequester
    {
        private string connectionId;

        private bool abandon = false;

        public RemoteServerInfo ServerInfo;

        protected Dictionary<string, string> labels = new Dictionary<string, string>();

        public Dictionary<string, string> GetLabels() => labels;

        public Task<CommonResponse> RequestAsync(CommonRequest req, CommonRequestMeta meta) => Request(req, meta);

        public Task<CommonResponse> RequestAsync(CommonRequest req, CommonRequestMeta meta, long timeoutMills) => Request(req, meta, timeoutMills);

        protected abstract Task<CommonResponse> Request(CommonRequest req, CommonRequestMeta meta);

        protected abstract Task<CommonResponse> Request(CommonRequest req, CommonRequestMeta meta, long timeoutMills);

        public void PutLabels(Dictionary<string, string> labels) => this.labels = labels;

        public void PutLabel(string labelName, string labelValue) => this.labels[labelName] = labelValue;

        public Task CloseAsync() => Close();

        protected abstract Task Close();

        public bool IsAbandon() => abandon;

        public void SetAbandon(bool abandon) => this.abandon = abandon;

        public string GetConnectionId() => connectionId;

        public void SetConnectionId(string connectionId) => this.connectionId = connectionId;
    }
}
